using System.Security;
using System.Security.Authentication;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using VkNet.Abstractions.Authorization;
using VkNet.Abstractions.Core;
using VkNet.Abstractions.Utils;
using VkNet.Extensions.Auth.Abstractions;
using VkNet.Extensions.Auth.Models.Auth;
using VkNet.Extensions.Auth.Models.LibVerify;
using VkNet.Extensions.Auth.Utils;
using VkNet.Extensions.DependencyInjection;
using VkNet.Extensions.DependencyInjection.Abstractions;
using VkNet.Model;
using VkNet.Utils;
using VkNet.Utils.JsonConverter;

namespace VkNet.Extensions.Auth.Flows;

internal abstract class AuthorizationFlowBase(
    IVkTokenStore tokenStore,
    FakeSafetyNetClient safetyNetClient,
    IDeviceIdStore deviceIdStore,
    IVkApiVersionManager versionManager,
    ILanguageService languageService,
    IAsyncRateLimiter rateLimiter,
    IRestClient restClient,
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

    public Task<AuthorizationResult> AuthorizeAsync()
    {
        if (_apiAuthParams == null)
            throw new InvalidOperationException("Authorization parameters are not set. Call SetAuthorizationParams first.");

        return AuthorizeAsync(_apiAuthParams);
    }

    protected abstract Task<AuthorizationResult> AuthorizeAsync(AndroidApiAuthParams authParams);

    public void SetAuthorizationParams(IApiAuthParams authorizationParams)
    {
        if (authorizationParams is not AndroidApiAuthParams authParams)
            throw new ArgumentException($"Authorization parameters must be of type {nameof(AndroidApiAuthParams)}", nameof(authorizationParams));
        
        _apiAuthParams = authParams;
    }

    protected async Task<AuthorizationResult> AuthAsync(AndroidApiAuthParams authParams)
    {
        if (authParams.IsAnonymous)
        {
            var (anonymousToken, anonymousTokenExpiration) = await AuthAnonymousAsync(authParams);

            return new()
            {
                State = authParams.State,
                ExpiresIn = anonymousTokenExpiration,
                AccessToken = anonymousToken
            };
        }

        return await captchaHandler.Perform(async (sid, key) =>
        {
            var parameters = await BuildParameters(authParams);
            
            parameters.Add("captcha_sid", sid);
            parameters.Add("captcha_key", key);
            
            await rateLimiter.WaitNextAsync();

            var response = await restClient.PostAsync(new("https://api.vk.com/oauth/token"), parameters, Encoding.UTF8);

            var obj = JObject.Parse(response.Value ?? response.Message);

            if (obj.TryGetValue("error", out var error) &&
                AuthFlow.FromJsonString(error.ToString()) == AuthFlow.NeedValidation)
            {
                var (loginWay, validationSid, mask, _, externalId) = obj.ToObject<NeedValidationAuthResponse>(_jsonSerializer);

                VerifyResponse verifyResponse = null;
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
                
                await rateLimiter.WaitNextAsync();

                response = await restClient.PostAsync(new("https://api.vk.com/oauth/token"), parameters, Encoding.UTF8);

                obj = JObject.Parse(response.Value ?? response.Message);
            }
            
            VkAuthErrors.IfErrorThrowException(obj);

            var result = obj.ToObject<AuthorizationResult>(_jsonSerializer);

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
            { "device_id", await GetDeviceIdAsync() },
            { "api_id", authParams.ApplicationId },
            { "https", true },
            { "lang", languageService.GetLanguage()?.ToString() ?? "ru" },
            { "v", versionManager.Version },
            { "anonymous_token", tokenStore.Token },
        };
    }

    private async Task<AnonymousTokenResponse> AuthAnonymousAsync(AndroidApiAuthParams authParams)
    {
        var parameters = new VkParameters
        {
            { "client_id", authParams.ApplicationId },
            { "api_id", authParams.ApplicationId },
            { "client_secret", authParams.ClientSecret },
            { "device_id", await GetDeviceIdAsync()},
            { "https", true },
            { "lang", languageService.GetLanguage()?.ToString() ?? "ru" },
            { "v", versionManager.Version }
        };

        var response = await restClient.PostAsync(new("https://api.vk.com/oauth/get_anonym_token"), parameters, Encoding.UTF8);
        
        var obj = VkErrors.IfErrorThrowException(response.Value ?? response.Message);
        VkAuthErrors.IfErrorThrowException(obj);

        return obj.ToObject<AnonymousTokenResponse>(_jsonSerializer);
    }

    private async ValueTask<string> GetDeviceIdAsync()
    {
        var deviceId = await deviceIdStore.GetDeviceIdAsync();
        if (!string.IsNullOrEmpty(deviceId))
            return deviceId;

        var checkIn = await safetyNetClient.CheckIn();

        var response = await safetyNetClient.Register(checkIn);

        deviceId = $"{checkIn.AndroidId}:{response.Split('=')[1]}";
        
        await deviceIdStore.SetDeviceIdAsync(deviceId);

        return deviceId;
    }
}