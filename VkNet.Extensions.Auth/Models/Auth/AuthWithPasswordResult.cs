namespace VkNet.Extensions.Auth.Models.Auth;

/// <summary>
/// Результат авторизации с паролём через auth.validateAccount.
/// Используется вместо AuthorizationResult из внешней библиотеки.
/// </summary>
public record AuthWithPasswordResult
{
    public string AccessToken { get; init; } = string.Empty;
    public long UserId { get; init; }
    public int ExpiresIn { get; init; }
    public string? State { get; init; }
}
