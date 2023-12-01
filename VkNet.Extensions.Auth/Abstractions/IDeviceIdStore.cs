namespace VkNet.Extensions.Auth.Abstractions;

public interface IDeviceIdStore
{
    ValueTask<string?> GetDeviceIdAsync();
    ValueTask SetDeviceIdAsync(string deviceId);
}