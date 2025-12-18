namespace VkNet.Extensions.Auth.Models.Auth;

public record AuthError(string Error, string? ErrorDescription, ulong? CaptchaSid, Uri? CaptchaImg, Uri? RedirectUri);