using VkNet.Abstractions.Core;
using VkNet.Abstractions.Utils;
using VkNet.Extensions.Auth.Abstractions;
using VkNet.Extensions.Auth.Models.Auth;
using VkNet.Extensions.Auth.Utils;
using VkNet.Extensions.DependencyInjection;
using VkNet.Extensions.DependencyInjection.Abstractions;
using VkNet.Model;
using VkNet.Utils;

namespace VkNet.Extensions.Auth.Flows;

internal class WithoutPasswordAuthorizationFlow(
    IVkTokenStore tokenStore,
    FakeSafetyNetClient safetyNetClient,
    IDeviceIdStore deviceIdStore,
    IVkApiVersionManager versionManager,
    ILanguageService languageService,
    IAsyncRateLimiter rateLimiter,
    IRestClient restClient,
    ICaptchaHandler captchaHandler,
    LibVerifyClient libVerifyClient)
    : AuthorizationFlowBase(tokenStore, safetyNetClient, deviceIdStore, versionManager,
        languageService, rateLimiter, restClient, captchaHandler, libVerifyClient)
{
    protected override Task<AuthorizationResult> AuthorizeAsync(AndroidApiAuthParams authParams)
    {
        return AuthAsync(authParams);
    }

    protected override async ValueTask<VkParameters> BuildParameters(AndroidApiAuthParams authParams)
    {
        var parameters = await base.BuildParameters(authParams);
        
        parameters.Add("grant_type", AndroidGrantType.WithoutPassword);
        parameters.Add("username", authParams.Login);
        parameters.Add("flow_type", "auth_without_password");
        parameters.Add("2fa_supported", true);
        parameters.Add("vk_connect_auth", true);
        
        return parameters;
    }
}