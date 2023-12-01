namespace VkNet.Extensions.DependencyInjection.Abstractions;

public interface IVkTokenStore
{
    string Token { get; }
    Task SetAsync(string? token, DateTimeOffset? expiration = null);
}