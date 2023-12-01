namespace VkNet.Extensions.Auth.Abstractions;

public interface IExchangeTokenStore
{
    ValueTask<string?> GetExchangeTokenAsync();
    ValueTask SetExchangeTokenAsync(string token);
}