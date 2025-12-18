using VkNet.Model;

namespace VkNet.Extensions.Auth.Exceptions;

/// <summary>
/// Ошибка входа в аккаунт VK
/// </summary>
/// <param name="vkAuthError">Объект ошибки, возвращенный сервером</param>
public class VkAuthException(VkAuthError vkAuthError)
	: System.Exception(vkAuthError.ErrorDescription ?? vkAuthError.Error)
{
	/// <summary>
	/// Объект ошибки, возвращенный сервером
	/// </summary>
	public VkAuthError AuthError { get; } = vkAuthError;
}