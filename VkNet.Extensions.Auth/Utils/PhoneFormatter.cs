namespace VkNet.Extensions.Auth.Utils;

/// <summary>
/// Утилита для форматирования телефонных номеров в формат E.164 для API VK.
/// Требования: минимум 3 символа, максимум 15 символов (включая +), формат: +[код страны][номер].
/// </summary>
public static class PhoneFormatter
{
    private const int MinPhoneLength = 3;
    private const int MaxPhoneLength = 15;

    /// <summary>
    /// Форматирует телефонный номер в формат E.164.
    /// </summary>
    /// <param name="phone">Телефонный номер в любом формате.</param>
    /// <returns>Телефонный номер в формате E.164 (например, +79123456789).</returns>
    public static string FormatToE164(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            throw new ArgumentException("Phone number cannot be empty", nameof(phone));

        // Удаляем все нецифровые символы кроме +
        var cleaned = phone.Trim();
        cleaned = RemoveNonDigitChars(cleaned);

        // Если номер начинается с 8 (российский формат)
        if (cleaned.StartsWith('8'))
        {
            cleaned = '7' + cleaned[1..];
        }

        // Добавляем + в начало, если его нет
        if (!cleaned.StartsWith('+'))
        {
            cleaned = '+' + cleaned;
        }

        // Валидация длины
        if (cleaned.Length < MinPhoneLength)
            throw new ArgumentException($"Phone number is too short. Minimum length is {MinPhoneLength} characters.", nameof(phone));

        if (cleaned.Length > MaxPhoneLength)
            throw new ArgumentException($"Phone number is too long. Maximum length is {MaxPhoneLength} characters.", nameof(phone));

        return cleaned;
    }

    /// <summary>
    /// Проверяет, является ли строка телефонным номером (начинается с + или цифры).
    /// </summary>
    public static bool IsPhoneNumber(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return false;

        var trimmed = input.Trim();
        return char.IsDigit(trimmed[0]) || trimmed[0] == '+';
    }

    /// <summary>
    /// Удаляет все нецифровые символы из строки, сохраняя + на первой позиции.
    /// </summary>
    private static string RemoveNonDigitChars(string phone)
    {
        if (string.IsNullOrEmpty(phone))
            return phone;

        var result = new System.Text.StringBuilder();
        var hasLeadingPlus = phone[0] == '+';

        foreach (var c in phone)
        {
            if (char.IsDigit(c))
            {
                result.Append(c);
            }
        }

        var cleaned = result.ToString();
        return hasLeadingPlus ? '+' + cleaned : cleaned;
    }
}
