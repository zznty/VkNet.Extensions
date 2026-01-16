using Microsoft.Win32;

namespace WpfApp.Services;

public class TokenChecker
{
    public bool IsTokenValid()
    {
        var token = Registry.CurrentUser.OpenSubKey("Software\\VkNet.Extensions.Auth")?.GetValue("Token")?.ToString();
        if (string.IsNullOrEmpty(token))
            return false;

        // Проверяем срок действия токена
        var expirationValue = Registry.CurrentUser.OpenSubKey("Software\\VkNet.Extensions.Auth")?.GetValue("TokenExpiration")?.ToString();
        if (!string.IsNullOrEmpty(expirationValue) && DateTimeOffset.TryParse(expirationValue, out var expiration))
        {
            // Токен истёк
            if (expiration <= DateTimeOffset.Now)
                return false;
        }
        else
        {
            // Если даты истечения нет - считаем токен невалидным для безопасности
            return false;
        }

        // Токен считается валидным только если есть и дата истечения
        return true;
    }
}