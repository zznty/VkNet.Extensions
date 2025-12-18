using Newtonsoft.Json.Linq;
using VkNet.Exception;
using VkNet.Extensions.Auth.Models.Auth;

namespace VkNet.Extensions.Auth.Utils;

public static class VkAuthErrors
{
	/// <summary>
	/// Выбрасывает ошибку, если есть в json.
	/// </summary>
	/// <param name="json"> JSON. </param>
	/// <exception cref="VkApiException">
	/// Неправильные данные JSON.
	/// </exception>
	public static void IfErrorThrowException(JToken json)
	{
		var error = json["error"];

		if (error == null || error.Type == JTokenType.Null)
		{
			return;
		}

		if (error.Type != JTokenType.String)
		{
			return;
		}

		var vkAuthError = json.ToObject<AuthError>(DependencyInjection.Services.VkApiInvoke.DefaultSerializer)!;

		throw VkAuthErrorFactory.Create(vkAuthError);
	}
}