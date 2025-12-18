using System.Security;
using System.Security.Authentication;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using VkNet.Abstractions.Authorization;
using VkNet.Abstractions.Core;
using VkNet.Extensions.Auth.Abstractions;
using VkNet.Extensions.Auth.Models.Auth;
using VkNet.Extensions.Auth.Models.LibVerify;
using VkNet.Extensions.Auth.Utils;
using VkNet.Extensions.DependencyInjection.Abstractions;
using VkNet.Model;
using VkNet.Utils;
using VkNet.Utils.JsonConverter;
using ICaptchaHandler = VkNet.Extensions.DependencyInjection.Abstractions.ICaptchaHandler;

namespace VkNet.Extensions.Auth.Flows;

internal abstract class AuthorizationFlowBase(
    IVkTokenStore tokenStore,
    IDeviceIdProvider deviceIdProvider,
    IVkApiVersionManager versionManager,
    ILanguageService languageService,
    IAsyncRateLimiter rateLimiter,
    HttpClient client,
    ICaptchaHandler captchaHandler,
    LibVerifyClient libVerifyClient)
    : IAuthorizationFlow
{
    private AndroidApiAuthParams? _apiAuthParams;
    
    private readonly JsonSerializer _jsonSerializer = JsonSerializer.Create(new()
    {
        Converters = new List<JsonConverter>
        {
            new VkCollectionJsonConverter(),
            new UnixDateTimeConverter(),
            new AttachmentJsonConverter(),
            new StringEnumConverter(),
        },
        ContractResolver = new DefaultContractResolver
        {
            NamingStrategy = new SnakeCaseNamingStrategy()
        },
        MaxDepth = null,
        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
    });

    public Task<AuthorizationResult> AuthorizeAsync(CancellationToken token = default)
    {
        if (_apiAuthParams == null)
            throw new InvalidOperationException("Authorization parameters are not set. Call SetAuthorizationParams first.");

        return AuthorizeAsync(_apiAuthParams, token);
    }

    protected abstract Task<AuthorizationResult> AuthorizeAsync(AndroidApiAuthParams authParams, CancellationToken token = default);

    public void SetAuthorizationParams(IApiAuthParams authorizationParams)
    {
        if (authorizationParams is not AndroidApiAuthParams authParams)
            throw new ArgumentException($"Authorization parameters must be of type {nameof(AndroidApiAuthParams)}", nameof(authorizationParams));
        
        _apiAuthParams = authParams;
    }

    protected async Task<AuthorizationResult> AuthAsync(AndroidApiAuthParams authParams, CancellationToken token = default)
    {
        if (authParams.IsAnonymous)
        {
            var (anonymousToken, anonymousTokenExpiration) = await AuthAnonymousAsync(authParams, token);

            return new()
            {
                State = authParams.State,
                ExpiresIn = anonymousTokenExpiration,
                AccessToken = anonymousToken
            };
        }

        return await captchaHandler.Perform(async captchaResponse =>
        {
            var parameters = await BuildParameters(authParams);
            
            captchaResponse?.AddTo(parameters);
            
            await rateLimiter.WaitNextAsync(token);

            var response = await client.PostAsync("https://api.vk.com/oauth/token", new FormUrlEncodedContent(parameters), token);
            await using var stream = await response.Content.ReadAsStreamAsync(token);
            using var reader = new StreamReader(stream);
            await using var jsonReader = new JsonTextReader(reader);

            var obj = await JToken.ReadFromAsync(jsonReader, token);
            
            if (obj["error"] is JObject error &&
                AuthFlow.FromJsonString(error.ToString()) == AuthFlow.NeedValidation)
            {
                var (loginWay, validationSid, mask, _, externalId) = obj.ToObject<NeedValidationAuthResponse>(_jsonSerializer)!;

                VerifyResponse? verifyResponse = null;
                if (loginWay == LoginWay.TwoFactorLibVerify)
                {
                    verifyResponse = await libVerifyClient.VerifyAsync(externalId, authParams.Login);

                    if (verifyResponse.Status != VerifyResponseStatus.Ok)
                        throw new VerificationException("Error verifying libverify session");
                }
                    

                var state = loginWay == LoginWay.TwoFactorLibVerify
                    ? new TwoFactorAuthState(validationSid, mask, verifyResponse!.Checks.Any(b => b == VerifyChecks.Sms),
                        verifyResponse.CodeLength)
                    : new AuthState(validationSid);

                var code = await authParams.CodeRequestedAsync!(loginWay, state);
                
                if (verifyResponse is null)
                {
                    parameters.Remove("code");
                    parameters.Add("code", code);
                }
                else
                {
                    var (status, token) = await libVerifyClient.AttemptAsync(verifyResponse.VerificationUrl, code);
                    
                    if (status != VerifyResponseStatus.Ok)
                        throw new AuthenticationException("Error attempting libverify code");

                    parameters.Remove("validate_session");
                    parameters.Remove("validate_token");
                    
                    parameters.Add("validate_session", verifyResponse.SessionId);
                    parameters.Add("validate_token", token);
                }
                
                await rateLimiter.WaitNextAsync(token);

                response = await client.PostAsync("https://api.vk.com/oauth/token", new FormUrlEncodedContent(parameters), token);
                await using var stream1 = await response.Content.ReadAsStreamAsync(token);
                using var reader1 = new StreamReader(stream);
                await using var jsonReader1 = new JsonTextReader(reader);

                obj = await JToken.ReadFromAsync(jsonReader, token);
            }
            
            VkAuthErrors.IfErrorThrowException(obj);

            var result = obj.ToObject<AuthorizationResult>(_jsonSerializer)!;

            result.State = authParams.State;
        
            return result;
        });
    }

    protected virtual async ValueTask<VkParameters> BuildParameters(AndroidApiAuthParams authParams)
    {
        return new()
        {
            { "libverify_support", false }, // TODO: test lib verify cringe
            { "sid", authParams.Sid },
            { "scope", "all" },
            { "supported_ways", authParams.SupportedWays },
            { "device_id", await deviceIdProvider.GetDeviceIdAsync() },
            { "api_id", authParams.ApplicationId },
            { "https", true },
            { "lang", languageService.GetLanguage()?.ToString() ?? "ru" },
            { "v", versionManager.Version },
            { "anonymous_token", tokenStore.Token },
        };
    }

    private async Task<AnonymousTokenResponse> AuthAnonymousAsync(AndroidApiAuthParams authParams, CancellationToken token = default)
    {
        var parameters = new VkParameters
        {
            { "client_id", authParams.ApplicationId },
            { "api_id", authParams.ApplicationId },
            { "client_secret", authParams.ClientSecret },
            { "device_id", await deviceIdProvider.GetDeviceIdAsync()},
            { "https", true },
            { "lang", languageService.GetLanguage()?.ToString() ?? "ru" },
            { "v", versionManager.Version }
        };

        using var response = await client.PostAsync("https://api.vk.com/oauth/get_anonym_token", new FormUrlEncodedContent(parameters), token);
        await using var stream = await response.Content.ReadAsStreamAsync(token);
        using var reader = new StreamReader(stream);
        await using var jsonReader = new JsonTextReader(reader);

        var obj = await JToken.ReadFromAsync(jsonReader, token);
        
        VkAuthErrors.IfErrorThrowException(obj);

        return obj.ToObject<AnonymousTokenResponse>(_jsonSerializer)!;
    }
}