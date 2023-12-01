using Microsoft.Win32;
using VkNet.Extensions.Auth.Abstractions;

namespace WpfApp.Services.Stores;

public class RegistryExchangeTokenStore : IExchangeTokenStore
{
    public ValueTask<string?> GetExchangeTokenAsync()
    {
        return new(Registry.CurrentUser.OpenSubKey("Software\\VkNet.Extensions.Auth")?.GetValue("ExchangeToken")?.ToString());
    }

    public ValueTask SetExchangeTokenAsync(string token)
    {
        var subKey = Registry.CurrentUser.OpenSubKey("Software\\VkNet.Extensions.Auth", true) ??
                      Registry.CurrentUser.CreateSubKey("Software\\VkNet.Extensions.Auth", true);
        
        subKey.SetValue("ExchangeToken", token, RegistryValueKind.String);
        
        return ValueTask.CompletedTask;
    }
}