namespace VkNet.Extensions.DependencyInjection.Abstractions;

/// <summary>
/// Хранилище токена доступа
/// </summary>
public interface IVkTokenStore
{
    /// <summary>
    /// Текущий сохраненный токен
    /// </summary>
    string Token { get; }
    /// <summary>
    /// Сохраняет новый токен в хранилище
    /// </summary>
    /// <param name="token">Новый токен</param>
    /// <param name="expiration">Необязательное время устаревания токена. <b>Токен может устареть до наступления установленного времени</b></param>
    Task SetAsync(string? token, DateTimeOffset? expiration = null);
}