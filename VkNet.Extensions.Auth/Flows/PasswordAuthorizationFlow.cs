using VkNet.Abstractions.Core;
using VkNet.Extensions.Auth.Abstractions;
using VkNet.Extensions.Auth.Abstractions.Categories;
using VkNet.Extensions.Auth.Models.Auth;
using VkNet.Extensions.Auth.Utils;
using VkNet.Extensions.DependencyInjection.Abstractions;
using VkNet.Model;
using VkNet.Utils;
using ICaptchaHandler = VkNet.Extensions.DependencyInjection.Abstractions.ICaptchaHandler;

namespace VkNet.Extensions.Auth.Flows;

internal class PasswordAuthorizationFlow(
    IVkTokenStore tokenStore,
    IDeviceIdProvider deviceIdProvider,
    IVkApiVersionManager versionManager,
    ILanguageService languageService,
    IAsyncRateLimiter rateLimiter,
    HttpClient client,
    ICaptchaHandler captchaHandler,
    LibVerifyClient libVerifyClient,
    IAuthCategory authCategory)
    : AuthorizationFlowBase(tokenStore, deviceIdProvider,
        versionManager, languageService, rateLimiter, client, captchaHandler, libVerifyClient)
{
    protected override async Task<AuthorizationResult> AuthorizeAsync(AndroidApiAuthParams authParams, CancellationToken token = default)
    {
        System.Diagnostics.Debug.WriteLine($"[PasswordAuthorizationFlow] AuthorizeAsync called. authParams.Phone: {(authParams.Phone ?? "NULL")}, authParams.Login: {(authParams.Login ?? "NULL")}");

        if (string.IsNullOrEmpty(authParams.Password) && !authParams.IsAnonymous)
            throw new ArgumentException("Password is required for this flow type", nameof(authParams));

        if (string.IsNullOrEmpty(authParams.Sid) && !authParams.IsAnonymous)
            throw new ArgumentException("SID is required for this flow type", nameof(authParams));

        // Используем captchaHandler.Perform для обработки капчи
        return await captchaHandler.Perform(async captchaResponse =>
        {
            // Получаем параметры капчи
            string? captchaSid = null;
            string? captchaKey = null;
            string? successToken = null;

            if (captchaResponse is ImageCaptchaResponse imageCaptcha)
            {
                captchaSid = imageCaptcha.Sid.ToString();
                captchaKey = imageCaptcha.Key;
            }
            else if (captchaResponse is BrowserCaptchaResponse browserCaptcha)
            {
                captchaSid = browserCaptcha.Sid.ToString();
                successToken = browserCaptcha.SuccessToken;
            }

            // Используем новый метод авторизации с паролём через API
            var result = await authCategory.ValidateAccountWithPasswordAsync(
                authParams.Login!,
                authParams.Password!,
                authParams.Sid!,
                captchaSid,
                captchaKey,
                successToken,
                token);

            // Конвертируем AuthWithPasswordResult в AuthorizationResult
            return new AuthorizationResult
            {
                AccessToken = result.AccessToken,
                UserId = result.UserId,
                ExpiresIn = result.ExpiresIn,
                State = result.State ?? authParams.State
            };
        });
    }

    protected override async ValueTask<VkParameters> BuildParameters(AndroidApiAuthParams authParams)
    {
        var parameters = await base.BuildParameters(authParams);

        parameters.Add("grant_type", AndroidGrantType.Password);
        // Используем user_name вместо username для соответствия с документацией API
        parameters.Add("user_name", authParams.Login);
        parameters.Add("password", authParams.Password);
        if (!string.IsNullOrEmpty(authParams.Code))
            parameters.Add("code", authParams.Code);
        // libverify_support должен быть true для корректной работы 2FA
        parameters.Remove("libverify_support");
        parameters.Add("libverify_support", true);

        return parameters;
    }
}