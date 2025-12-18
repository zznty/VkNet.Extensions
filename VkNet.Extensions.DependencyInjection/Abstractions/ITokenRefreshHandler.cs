namespace VkNet.Extensions.DependencyInjection.Abstractions;

/// <summary>
/// Обработчик обновления токена доступа в случае его устаревания
/// </summary>
public interface ITokenRefreshHandler
{
    /// <summary>
    /// Обновляет устаревший токен
    /// </summary>
    /// <param name="oldToken">Устаревший токен</param>
    /// <returns>Новый токен доступа</returns>
    /// <remarks>В случае успеха операции так же неявно обновляется токен в глоабальном хранилище токена <see cref="IVkTokenStore"/></remarks>
    Task<string?> RefreshTokenAsync(string oldToken);
}