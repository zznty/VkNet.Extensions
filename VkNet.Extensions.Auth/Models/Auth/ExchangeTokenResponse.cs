using VkNet.Model;

namespace VkNet.Extensions.Auth.Models.Auth;

public record ExchangeTokenResponse(string ExchangeToken, User Profile);