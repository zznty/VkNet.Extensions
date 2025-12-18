namespace VkNet.Extensions.Auth.Abstractions;

public interface IDeviceIdProvider
{
    ValueTask<string> GetDeviceIdAsync();
}