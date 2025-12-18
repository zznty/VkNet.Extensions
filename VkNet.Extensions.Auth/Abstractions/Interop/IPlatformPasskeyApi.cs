using VkNet.Extensions.Auth.Models.Auth;

namespace VkNet.Extensions.Auth.Abstractions.Interop;

/// <summary>
/// Предоставляет поддержку входа с помощью Passkey (WebAuthN)
/// </summary>
/// <remarks>В брендинде VK это называется "OnePass", "Вход по скану лица или отпечатку пальца", "Электронный ключ"</remarks>
public interface IPlatformPasskeyApi
{
    /// <summary>
    /// Производит подпись, представляющую утверждение аутентификатора о том, что пользователь дал согласие на конкретную транзакцию, такую как вход в систему. 
    /// </summary>
    /// <param name="passkeyData">Параметры запроса подписи</param>
    /// <param name="origin">Идентификатор запрашиваемой подписи</param>
    /// <returns>JSON строка утверждения аутентификатора о согласии пользователя на конкретную транзакцию</returns>
    Task<string> RequestPasskeyAsync(PasskeyDataResponse passkeyData, string origin);
}