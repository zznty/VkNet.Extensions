namespace VkNet.Extensions.DependencyInjection.Abstractions;

public interface ITokenRefreshHandler
{
    Task<string?> RefreshTokenAsync(string oldToken);
}