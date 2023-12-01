using VkNet.Model;

namespace VkNet.Extensions.Auth.Exceptions;

public class VkAuthException(VkAuthError vkAuthError)
	: System.Exception(vkAuthError.ErrorDescription ?? vkAuthError.Error)
{
	public VkAuthError AuthError { get; } = vkAuthError;
}