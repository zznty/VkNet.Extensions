using VkNet.Abstractions.Core;
using VkNet.Extensions.Auth.Abstractions;
using VkNet.Extensions.Auth.Models.Auth;
using VkNet.Extensions.Auth.Utils;
using VkNet.Extensions.DependencyInjection.Abstractions;
using VkNet.Model;
using VkNet.Utils;
using ICaptchaHandler = VkNet.Extensions.DependencyInjection.Abstractions.ICaptchaHandler;

namespace VkNet.Extensions.Auth.Flows;

internal class PasskeyAuthorizationFlow(
    IVkTokenStore tokenStore,
    IDeviceIdProvider deviceIdProvider,
    IVkApiVersionManager versionManager,
    ILanguageService languageService,
    IAsyncRateLimiter rateLimiter,
    HttpClient client,
    ICaptchaHandler captchaHandler,
    LibVerifyClient libVerifyClient)
    : AuthorizationFlowBase(tokenStore, deviceIdProvider, versionManager,
        languageService, rateLimiter, client, captchaHandler, libVerifyClient)
{
    protected override Task<AuthorizationResult> AuthorizeAsync(AndroidApiAuthParams authParams, CancellationToken token = default)
    {
        if (string.IsNullOrEmpty(authParams.PasskeyData))
            throw new ArgumentException("Passkey data is empty", nameof(authParams));
        
        return AuthAsync(authParams, token);
    }

    protected override async ValueTask<VkParameters> BuildParameters(AndroidApiAuthParams authParams)
    {
        var parameters = await base.BuildParameters(authParams);
        
        parameters.Add("grant_type", AndroidGrantType.Passkey);
        parameters.Add("passkey_data", authParams.PasskeyData);
        parameters.Add("flow_type", "tg_flow");
        
        return parameters;
    }
}