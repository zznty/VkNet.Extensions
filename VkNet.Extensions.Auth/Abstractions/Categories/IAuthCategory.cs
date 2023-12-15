using VkNet.Enums.Filters;
using VkNet.Extensions.Auth.Models.Auth;

namespace VkNet.Extensions.Auth.Abstractions.Categories;

public interface IAuthCategory
{
    Task<AuthValidateAccountResponse> ValidateAccountAsync(string login, bool forcePassword = false, bool passkeySupported = false, IEnumerable<LoginWay>? loginWays = null, CancellationToken token = default);
    Task<AuthValidatePhoneResponse> ValidatePhoneAsync(string phone, string sid, bool allowCallReset = true, IEnumerable<LoginWay>? loginWays = null, CancellationToken token = default);

    Task<AuthCodeResponse> GetAuthCodeAsync(string deviceName, bool forceRegenerate = true, CancellationToken token = default);
    
    Task<AuthCheckResponse> CheckAuthCodeAsync(string authHash, CancellationToken token = default);
    
    Task<TokenInfo?> RefreshTokensAsync(string oldToken, string exchangeToken, CancellationToken token = default);

    Task<ExchangeTokenResponse> GetExchangeToken(UsersFields? fields = null, CancellationToken token = default);
    
    Task<PasskeyBeginResponse> BeginPasskeyAsync(string sid, CancellationToken token = default);
}