namespace VkNet.Extensions.Auth.Abstractions;

/// <summary>
/// Предоставляет уникальный идентификатор устройства
/// </summary>
public interface IDeviceIdProvider
{
    /// <summary>
    /// Предоставляет уникальный идентификатор устройства
    /// </summary>
    /// <returns>Уникальный идентификатор устройства</returns>
    /// <remarks>Значение желательно кешировать из-за частоты вызовов</remarks>
    ValueTask<string> GetDeviceIdAsync();
}