namespace VkNet.Extensions.Auth.Models.Auth;

public record AnonymousTokenResponse(string Token, int ExpiredAt);