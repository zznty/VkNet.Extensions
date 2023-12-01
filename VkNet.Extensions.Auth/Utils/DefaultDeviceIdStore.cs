using VkNet.Extensions.Auth.Abstractions;

namespace VkNet.Extensions.Auth.Utils;

public class DefaultDeviceIdStore : IDeviceIdStore
{
    private string? _deviceId;
    
    public ValueTask<string?> GetDeviceIdAsync()
    {
        return new(_deviceId);
    }

    public ValueTask SetDeviceIdAsync(string deviceId)
    {
        _deviceId = deviceId;
        return ValueTask.CompletedTask;
    }
}