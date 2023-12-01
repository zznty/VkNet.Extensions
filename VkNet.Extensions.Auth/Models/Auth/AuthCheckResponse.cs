namespace VkNet.Extensions.Auth.Models.Auth;

public record AuthCheckResponse(AuthCheckStatus Status, int ExpiresIn, string? SuperAppToken, string? AccessToken, bool NeedPassword, bool IsPartial, int ProviderAppId);

public enum AuthCheckStatus
{
    Continue,
    ConfirmOnPhone,
    Ok,
    Expired = 4,
    Loading = 200
}