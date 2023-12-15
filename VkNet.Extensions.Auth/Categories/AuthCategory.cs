using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;
using VkNet.Abstractions;
using VkNet.Abstractions.Core;
using VkNet.Enums.Filters;
using VkNet.Extensions.Auth.Models.Auth;
using VkNet.Extensions.DependencyInjection.Abstractions;
using VkNet.Utils;
using Categories_IAuthCategory = VkNet.Extensions.Auth.Abstractions.Categories.IAuthCategory;

namespace VkNet.Extensions.Auth.Categories;

public partial class AuthCategory(
    IVkApiInvoke apiInvoke,
    HttpClient client,
    IVkTokenStore tokenStore,
    IVkApiVersionManager versionManager)
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
}