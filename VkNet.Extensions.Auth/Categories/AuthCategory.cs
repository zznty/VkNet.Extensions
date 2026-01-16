using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;
using VkNet.Abstractions;
using VkNet.Abstractions.Authorization;
using VkNet.Abstractions.Core;
using VkNet.Enums.Filters;
using VkNet.Extensions.Auth.Abstractions;
using VkNet.Extensions.Auth.Models.Auth;
using VkNet.Extensions.Auth.Utils;
using VkNet.Extensions.DependencyInjection.Abstractions;
using VkNet.Model;
using VkNet.Utils;
using Categories_IAuthCategory = VkNet.Extensions.Auth.Abstractions.Categories.IAuthCategory;

namespace VkNet.Extensions.Auth.Categories;

public partial class AuthCategory(
    IVkApiInvoke apiInvoke,
    HttpClient client,
    IVkTokenStore tokenStore,
    IVkApiVersionManager versionManager,
    IDeviceIdProvider deviceIdProvider)
    : Categories_IAuthCategory
{
    private string? _anonToken;
    private string? _authVerifyHash;

    [GeneratedRegex("""\"anonymous_token\"\:\s?\"(?<token>[\w\.\=\-]*)\"\,?""", RegexOptions.Multiline)]
    private static partial Regex AnonTokenRegex();
    
    [GeneratedRegex("""\"code_auth_verification_hash\"\:\s?\"(?<hash>[\w\.\=\-]*)\"\,?""", RegexOptions.Multiline)]
    private static partial Regex AuthVerifyHashRegex();

    private async ValueTask<string> GetAnonTokenAsync(CancellationToken token = default)
    {
        const string url =
            "https://id.vk.com/qr_auth?scheme=vkcom_dark&app_id=7913379&origin=https%3A%2F%2Fvk.com&initial_stats_info=eyJzb3VyY2UiOiJtYWluIiwic2NyZWVuIjoic3RhcnQifQ%3D%3D";

        var response = await client.GetStringAsync(url, token);

        _authVerifyHash = AuthVerifyHashRegex().Match(response).Groups["hash"].Value;
        
        return _anonToken = AnonTokenRegex().Match(response).Groups["token"].Value;
    }

    public Task<AuthValidateAccountResponse> ValidateAccountAsync(string login, bool forcePassword = false, bool passkeySupported = false, IEnumerable<LoginWay>? loginWays = null, CancellationToken token = default)
    {
        return apiInvoke.CallAsync<AuthValidateAccountResponse>("auth.validateAccount", new()
        {
            { "login", login },
            { "force_password", forcePassword },
            { "supported_ways", loginWays },
            { "flow_type", "auth_without_password" },
            { "api_id", 2274003 },
            { "passkey_supported", passkeySupported }
        }, token: token);
    }

    public Task<AuthValidatePhoneResponse> ValidatePhoneAsync(string phone, string sid, bool allowCallReset = true,
        IEnumerable<LoginWay>? loginWays = null, CancellationToken token = default)
    {
        return apiInvoke.CallAsync<AuthValidatePhoneResponse>("auth.validatePhone", new()
        {
            { "phone", phone },
            { "sid", sid },
            { "supported_ways", loginWays },
            { "flow_type", "tg_flow" },
            { "allow_callreset", allowCallReset }
        }, token: token);
    }

    public async Task<AuthCodeResponse> GetAuthCodeAsync(string deviceName, bool forceRegenerate = true,
        CancellationToken token = default)
    {
        return await apiInvoke.CallAsync<AuthCodeResponse>("auth.getAuthCode", new()
        {
            { "device_name", deviceName },
            { "force_regenerate", forceRegenerate },
            { "auth_code_flow", false },
            { "client_id", 7913379 },
            { "anonymous_token", _anonToken ?? await GetAnonTokenAsync(token) },
            { "verification_hash", _authVerifyHash }
        }, true, token);
    }

    public async Task<AuthCheckResponse> CheckAuthCodeAsync(string authHash, CancellationToken token = default)
    {
        return await apiInvoke.CallAsync<AuthCheckResponse>("auth.checkAuthCode", new()
        {
            { "auth_hash", authHash },
            { "client_id", 7913379 },
            { "anonymous_token", _anonToken ?? await GetAnonTokenAsync(token) }
        }, true, token);
    }

    public async Task<TokenInfo?> RefreshTokensAsync(string oldToken, string exchangeToken, CancellationToken token = default)
    {
        var response = await apiInvoke.CallAsync<AuthRefreshTokensResponse>("auth.refreshTokens", new()
        {
            { "exchange_tokens", exchangeToken },
            { "scope", "all" },
            {"initiator", "expired_token"},
            {"active_index", 0},
            { "api_id", 2274003 },
            { "client_id", 2274003 },
            { "client_secret", "hHbZxrka2uZ6jB1inYsH" },
        }, true, token);
        
        return response.Success.Count > 0 ? response.Success[0].AccessToken : null;
    }

    public Task<ExchangeTokenResponse> GetExchangeToken(UsersFields? fields = null, CancellationToken token = default)
    {
        return apiInvoke.CallAsync<ExchangeTokenResponse>("execute.getUserInfo", new()
        {
            { "func_v", 30 },
            { "androidVersion", 32 },
            { "androidManufacturer", "MusicX" },
            { "androidModel", "MusicX" },
            { "needExchangeToken", true },
            { "fields", fields }
        }, token: token);
    }

    public async Task<PasskeyBeginResponse> BeginPasskeyAsync(string sid, CancellationToken token = default)
    {
        using var response = await client.PostAsync("https://api.vk.com/oauth/passkey_begin", new FormUrlEncodedContent(new VkParameters
        {
            { "sid", sid },
            { "anonymous_token", tokenStore.Token },
            { "v", versionManager.Version },
            { "https", true }
        }), token);

        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
        };

        return (await response.Content.ReadFromJsonAsync<PasskeyBeginResponse>(options, cancellationToken: token))!;
    }

    /// <summary>
    /// Авторизация с паролём через API метод auth.validateAccount.
    /// Использует JSON Content-Type вместо FormUrlEncoded.
    /// </summary>
    public async Task<AuthWithPasswordResult> ValidateAccountWithPasswordAsync(string login, string password, string sid,
        string? captchaSid = null, string? captchaKey = null, string? successToken = null, CancellationToken token = default)
    {
        // Получаем device_id
        var deviceId = await deviceIdProvider.GetDeviceIdAsync();

        // Форматируем телефон в E.164, если это номер телефона (не email)
        // Email оставляем как есть, телефон форматируем: 89123456789 -> +79123456789
        var formattedLogin = PhoneFormatter.IsPhoneNumber(login)
            ? PhoneFormatter.FormatToE164(login)
            : login;

        System.Diagnostics.Debug.WriteLine($"[auth.validateAccount] original login: {login}, formatted: {formattedLogin}");

        var payload = new
        {
            login = formattedLogin,
            //phone = formattedLogin,  // Используем отформатированный номер телефона в формате E.164
            password = password,
            sid = sid,
            device_id = deviceId
        };

        // Логируем запрос для отладки
        var options = new System.Text.Json.JsonSerializerOptions
        {
            // Отключаем экранирование для избежания проблем с API VK
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            WriteIndented = false
        };
        var payloadJson = System.Text.Json.JsonSerializer.Serialize(payload, options);
        System.Diagnostics.Debug.WriteLine($"[auth.validateAccount] Request payload: {payloadJson}");
        System.Diagnostics.Debug.WriteLine($"[auth.validateAccount] CaptchaSid: {captchaSid}, CaptchaKey: {captchaKey}, SuccessToken: {successToken?.Substring(0, Math.Min(50, successToken?.Length ?? 0))}...");

        // Формируем URL с параметрами
        var urlBuilder = $"https://api.vk.com/method/auth.validateAccount?access_token={Uri.EscapeDataString(tokenStore.Token)}&v={versionManager.Version}";
        if (!string.IsNullOrEmpty(captchaSid))
            urlBuilder += $"&captcha_sid={captchaSid}";
        if (!string.IsNullOrEmpty(captchaKey))
            urlBuilder += $"&captcha_key={Uri.EscapeDataString(captchaKey)}";
        if (!string.IsNullOrEmpty(successToken))
            urlBuilder += $"&success_token={Uri.EscapeDataString(successToken)}";

        System.Diagnostics.Debug.WriteLine($"[auth.validateAccount] URL: {urlBuilder}");

        using var request = new HttpRequestMessage(HttpMethod.Post, urlBuilder);
        request.Content = new StringContent(
            System.Text.Json.JsonSerializer.Serialize(payload),
            System.Text.Encoding.UTF8,
            "application/json"
        );

        using var response = await client.SendAsync(request, token);
        var responseContent = await response.Content.ReadAsStringAsync(token);

        // Логируем ответ для отладки
        System.Diagnostics.Debug.WriteLine($"[auth.validateAccount] Response: {responseContent}");

        // Парсим JSON ответ с помощью Newtonsoft.Json для совместимости с существующим кодом
        var json = Newtonsoft.Json.Linq.JToken.Parse(responseContent);

        // Проверяем на наличие ошибок
        var error = json["error"];
        if (error is not null)
        {
            var errorCodeStr = error["error_code"]?.ToString();
            System.Diagnostics.Debug.WriteLine($"[auth.validateAccount] Error: {errorCodeStr}");
            if (int.TryParse(errorCodeStr, out var errorCode) && errorCode == 14)
            {
                // Капча - выбрасываем CaptchaRequiredException
                var captchaSidValue = error["captcha_sid"]?.ToString();
                var captchaImg = error["captcha_img"]?.ToString();
                var redirectUriStr = error["redirect_uri"]?.ToString();

                System.Diagnostics.Debug.WriteLine($"[auth.validateAccount] Captcha required! Sid: {captchaSidValue}, Img: {captchaImg}, RedirectUri: {redirectUriStr}");

                if (ulong.TryParse(captchaSidValue, out var captchaSidNum))
                {
                    throw new DependencyInjection.CaptchaRequiredException(new VkNet.Model.VkError
                    {
                        ErrorCode = 14,
                        ErrorMessage = "Captcha needed",
                        CaptchaSid = captchaSidNum,
                        CaptchaImg = !string.IsNullOrEmpty(captchaImg) ? new Uri(captchaImg) : null,
                        RedirectUri = !string.IsNullOrEmpty(redirectUriStr) ? new Uri(redirectUriStr) : null
                    });
                }
            }

            var errorMsg = error["error_msg"]?.ToString();
            throw new VkNet.Exception.VkApiException($"auth.validateAccount failed: ErrorCode={errorCodeStr}, ErrorMessage={errorMsg}, FullResponse={responseContent}");
        }

        VkAuthErrors.IfErrorThrowException(json);

        // Если ответ содержит токен - возвращаем AuthWithPasswordResult
        var accessToken = json["response"]?["access_token"]?.ToString();
        if (!string.IsNullOrEmpty(accessToken))
        {
            return new AuthWithPasswordResult
            {
                AccessToken = accessToken,
                UserId = long.Parse(json["response"]?["user_id"]?.ToString() ?? "0"),
                ExpiresIn = int.Parse(json["response"]?["expires_in"]?.ToString() ?? "86400"),
                State = null
            };
        }

        // Если токена нет - возможна необходимость дальнейших шагов
        throw new VkNet.Exception.VkApiException("auth.validateAccount with password did not return access token. Response: " + responseContent);
    }
}