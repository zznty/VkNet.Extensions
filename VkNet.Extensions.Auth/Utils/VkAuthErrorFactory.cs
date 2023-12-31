using VkNet.Exception;
using VkNet.Extensions.Auth.Exceptions;
using VkNet.Model;

namespace VkNet.Extensions.Auth.Utils;

public static class VkAuthErrorFactory
{
	public static System.Exception Create(VkAuthError error)
	{
		switch (error.Error)
		{
			case "need_captcha":
				return new CaptchaNeededException(new()
				{
					CaptchaImg = error.CaptchaImg,
					CaptchaSid = error.CaptchaSid.GetValueOrDefault(),
					ErrorMessage = error.ErrorDescription
				});
			default:
				return new VkAuthException(error);
		}
	}
}