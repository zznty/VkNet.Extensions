using VkNet.Abstractions.Core;
using VkNet.Extensions.Auth.Abstractions;
using VkNet.Extensions.DependencyInjection.Abstractions;
using ICaptchaHandler = VkNet.Extensions.DependencyInjection.Abstractions.ICaptchaHandler;

namespace VkNet.Extensions.Auth.Utils;

public class VkApiInvoke(
    HttpClient client,
    ICaptchaHandler handler,
    IVkApiVersionManager versionManager,
    IVkTokenStore tokenStore,
    ILanguageService languageService,
    IAsyncRateLimiter rateLimiter,
    ITokenRefreshHandler tokenRefreshHandler,
    IDeviceIdProvider deviceIdProvider)
    : VkNet.Extensions.DependencyInjection.Services.VkApiInvoke(client, handler, versionManager, tokenStore,
        languageService, rateLimiter, tokenRefreshHandler)
{
    protected override async ValueTask TryAddRequiredParametersAsync(IDictionary<string, string> parameters, bool skipAuthorization)
    {
        await base.TryAddRequiredParametersAsync(parameters, skipAuthorization);
        parameters.TryAdd("device_id", await deviceIdProvider.GetDeviceIdAsync());
    }
}