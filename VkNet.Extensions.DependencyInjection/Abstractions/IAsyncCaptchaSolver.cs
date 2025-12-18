namespace VkNet.Extensions.DependencyInjection.Abstractions;

/// <summary>
/// Определяет интерфейс взаимодействия с сервисом-распознавателем капчи
/// </summary>
public interface IAsyncCaptchaSolver
{
    ValueTask<string?> SolveAsync(CaptchaRequest request);

    /// <summary>
    /// Сообщает сервису, что последняя капча была распознана неверно.
    /// </summary>
    ValueTask SolveFailedAsync();
}

/// <summary>
/// Абстрактный объект запроса капчи.
/// Может быть запросом решения кода с изображения <see cref="ImageCaptchaRequest"/> или новой браузерной версии <see cref="BrowserCaptchaRequest"/>>
/// </summary>
public abstract record CaptchaRequest;

/// <summary>
/// Запрос капчи в виде решения кода с изображения
/// </summary>
/// <param name="ImageUri">Адрес изображения с кодом</param>
public record ImageCaptchaRequest(Uri ImageUri) : CaptchaRequest;
/// <summary>
/// Запрос капчи в виде решения задачи в браузере
/// </summary>
/// <param name="RedirectUri">Прямой адрес задачи в браузере</param>
public record BrowserCaptchaRequest(Uri RedirectUri) : CaptchaRequest;