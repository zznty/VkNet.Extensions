namespace VkNet.Extensions.Auth.Abstractions;

/// <summary>
/// Хранилище exchange (refresh) токена
/// </summary>
public interface IExchangeTokenStore
{
    /// <summary>
    /// Возвращает сохраненный exchange токен
    /// </summary>
    /// <returns>Сохраненный exchange токен</returns>
    ValueTask<string?> GetExchangeTokenAsync();
    
    /// <summary>
    /// Сохраняет exchange токен
    /// </summary>
    /// <param name="token">Exchange токен</param>
    ValueTask SetExchangeTokenAsync(string token);
}