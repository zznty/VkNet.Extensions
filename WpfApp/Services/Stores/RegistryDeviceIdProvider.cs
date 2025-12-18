using Microsoft.Win32;
using VkNet.Extensions.Auth.Abstractions;

namespace WpfApp.Services.Stores;

public class RegistryDeviceIdProvider : IDeviceIdProvider
{
    public ValueTask<string?> GetDeviceIdAsync()
    {
        return new(Registry.CurrentUser.OpenSubKey("Software\\VkNet.Extensions.Auth")?.GetValue("DeviceId")?.ToString());
    }

    public ValueTask SetDeviceIdAsync(string deviceId)
    {
        var subKey = Registry.CurrentUser.OpenSubKey("Software\\VkNet.Extensions.Auth", true) ??
                     Registry.CurrentUser.CreateSubKey("Software\\VkNet.Extensions.Auth", true);
        
        subKey.SetValue("DeviceId", deviceId, RegistryValueKind.String);
        
        return ValueTask.CompletedTask;
    }
}