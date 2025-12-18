namespace VkNet.Extensions.DependencyInjection.Abstractions;

/// <summary>
/// Предоставляет асинхронное ограничение максимального количества запросов в установленный период
/// </summary>
public interface IAsyncRateLimiter : IDisposable
{
    /// <summary>
    /// Период сброса ограничения
    /// </summary>
    public TimeSpan Window { get; }
    /// <summary>
    /// Максимальное количество запросов в установленный период <see cref="Window"/>
    /// </summary>
    public int MaxRequestsPerWindow { get; }
    /// <summary>
    /// Асинхронное ожидание следующего доступного периода
    /// </summary>
    /// <remarks>
    /// Асинхронная блокировка происходит только в случае превышения максимального количества запросов <see cref="MaxRequestsPerWindow"/> в установленный период <see cref="Window"/>
    /// </remarks>
    /// <param name="cancellationToken">Токен отмены</param>
    ValueTask WaitNextAsync(CancellationToken cancellationToken = default);
    /// <summary>
    /// Асинхронное ожидание следующего доступного периода
    /// </summary>
    /// <remarks>
    /// Асинхронная блокировка происходит только в случае превышения максимального количества запросов <see cref="MaxRequestsPerWindow"/> в установленный период <see cref="Window"/>
    /// </remarks>
    /// <param name="timeout">Тайм-аут ожидания в случае асинхронной блокировки</param>
    /// <returns>Возвращает <see langword="true"/> в случае успешного ожидания периода или <see langword="false"/> в случае достижения тайм-аута</returns>
    ValueTask<bool> WaitNextAsync(int timeout);
    /// <summary>
    /// Асинхронное ожидание следующего доступного периода
    /// </summary>
    /// <remarks>
    /// Асинхронная блокировка происходит только в случае превышения максимального количества запросов <see cref="MaxRequestsPerWindow"/> в установленный период <see cref="Window"/>
    /// </remarks>
    /// <param name="timeout">Тайм-аут ожидания в случае асинхронной блокировки</param>
    /// <returns>Возвращает <see langword="true"/> в случае успешного ожидания периода или <see langword="false"/> в случае достижения тайм-аута</returns>
    ValueTask<bool> WaitNextAsync(TimeSpan timeout);
    /// <summary>
    /// Попытка получения слудующего доступного периода
    /// </summary>
    /// <returns>Возвращает <see langword="true"/> в случае успешного получения периода</returns>
    bool TryGetNext();
}