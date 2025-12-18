using VkNet.Exception;
using VkNet.Extensions.DependencyInjection.Abstractions;
using VkNet.Model;

namespace VkNet.Extensions.DependencyInjection;

/// <summary>
/// Исключение, выбрасываемое при необходимости ввода капчи для вызова метода
/// </summary>
/// <remarks>
/// Код ошибки - 14
/// </remarks>
/// <seealso cref="IAsyncCaptchaSolver"/>
public class CaptchaRequiredException(VkError error) : VkApiMethodInvokeException(error)
{
    public VkError Error { get; } = error;
}