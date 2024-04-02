using VkNet.Abstractions.Core;
using VkNet.Extensions.Auth.Abstractions;
using VkNet.Extensions.DependencyInjection.Abstractions;

namespace VkNet.Extensions.Auth.Utils;

public class VkApiInvoke(
    HttpClient client,
    ICaptchaHandler handler,
    IVkApiVersionManager versionManager,
    IVkTokenStore tokenStore,
    ILanguageService languageService,
    IAsyncRateLimiter rateLimiter,
    ITokenRefreshHandler tokenRefreshHandler,
    IDeviceIdStore deviceIdStore)
    : VkNet.Extensions.DependencyInjection.Services.VkApiInvoke(client, handler, versionManager, tokenStore, languageService, rateLimiter, tokenRefreshHandler)
{
    protected override async ValueTask TryAddRequiredParametersAsync(IDictionary<string, string> parameters, bool skipAuthorization)
    {
        await base.TryAddRequiredParametersAsync(parameters, skipAuthorization);
        
        if (await deviceIdStore.GetDeviceIdAsync() is { } deviceId)
            parameters.TryAdd("device_id", deviceId);
    }
}