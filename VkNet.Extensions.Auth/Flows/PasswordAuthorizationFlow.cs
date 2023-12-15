using VkNet.Abstractions.Core;
using VkNet.Extensions.Auth.Abstractions;
using VkNet.Extensions.Auth.Models.Auth;
using VkNet.Extensions.Auth.Utils;
using VkNet.Extensions.DependencyInjection.Abstractions;
using VkNet.Model;
using VkNet.Utils;

namespace VkNet.Extensions.Auth.Flows;

internal class PasswordAuthorizationFlow(
    IVkTokenStore tokenStore,
    FakeSafetyNetClient safetyNetClient,
    IDeviceIdStore deviceIdStore,
    IVkApiVersionManager versionManager,
    ILanguageService languageService,
    IAsyncRateLimiter rateLimiter,
    HttpClient client,
    ICaptchaHandler captchaHandler,
    LibVerifyClient libVerifyClient)
    : AuthorizationFlowBase(tokenStore, safetyNetClient, deviceIdStore,
        versionManager, languageService, rateLimiter, client, captchaHandler, libVerifyClient)
{
    protected override Task<AuthorizationResult> AuthorizeAsync(AndroidApiAuthParams authParams, CancellationToken token = default)
    {
        if (string.IsNullOrEmpty(authParams.Password) && !authParams.IsAnonymous)
            throw new ArgumentException("Password is required for this flow type", nameof(authParams));
        
        return AuthAsync(authParams, token);
    }

    protected override async ValueTask<VkParameters> BuildParameters(AndroidApiAuthParams authParams)
    {
        var parameters = await base.BuildParameters(authParams);
        
        parameters.Add("grant_type", AndroidGrantType.Password);
        parameters.Add("username", authParams.Login);
        parameters.Add("password", authParams.Password);
        parameters.Add("flow_type", "tg_flow");
        parameters.Add("2fa_supported", true);
        parameters.Add("vk_connect_auth", true);
        
        return parameters;
    }
}