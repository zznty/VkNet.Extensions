using Newtonsoft.Json.Linq;
using VkNet.Abstractions.Core;
using VkNet.Extensions.Auth.Abstractions;
using VkNet.Extensions.Auth.Models.Auth;
using VkNet.Extensions.DependencyInjection;
using VkNet.Extensions.DependencyInjection.Abstractions;
using VkNet.Model;
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
    protected override async ValueTask TryAddRequiredParametersAsync(IDictionary<string, string> parameters)
    {
        await base.TryAddRequiredParametersAsync(parameters);
        parameters.TryAdd("device_id", await deviceIdProvider.GetDeviceIdAsync());
    }

    protected override void ThrowVkError(JToken error, VkError vkError)
    {
        if (vkError.ErrorCode == 14)
        {
            var authError = error.ToObject<AuthError>(DefaultSerializer)!;
            throw new CaptchaRequiredException(new VkError
            {
                ErrorCode = vkError.ErrorCode,
                ErrorMessage = vkError.ErrorMessage,
                CaptchaImg = authError.CaptchaImg,
                CaptchaSid = authError.CaptchaSid.GetValueOrDefault(),
                RedirectUri = authError.RedirectUri
            });
        }
        base.ThrowVkError(error, vkError);
    }
}