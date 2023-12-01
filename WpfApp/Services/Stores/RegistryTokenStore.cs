using System.Globalization;
using Microsoft.Win32;
using VkNet.Extensions.DependencyInjection.Abstractions;

namespace WpfApp.Services.Stores;

public class RegistryTokenStore : IVkTokenStore
{
    private string? _token;
    
    private DateTimeOffset? Expiration
    {
        get
        {
            var value = Registry.CurrentUser.OpenSubKey("Software\\VkNet.Extensions.Auth")?.GetValue("TokenExpiration")?.ToString();
            return value is null ? null : DateTimeOffset.Parse(value, CultureInfo.InvariantCulture);
        }

        set
        {
            var subKey = Registry.CurrentUser.OpenSubKey("Software\\VkNet.Extensions.Auth", true) ?? 
                         Registry.CurrentUser.CreateSubKey("Software\\VkNet.Extensions.Auth", true);
            
            subKey.SetValue("TokenExpiration", (value ?? DateTimeOffset.MaxValue).ToString(CultureInfo.InvariantCulture), RegistryValueKind.String);
        }
    }

    public string Token
    {
        get
        {
            var token = _token ??= Registry.CurrentUser.OpenSubKey("Software\\VkNet.Extensions.Auth")?.GetValue("Token")?.ToString() ?? throw new InvalidOperationException("Authorization is required");
            
            var expiration = Expiration;
            if (expiration.HasValue && expiration.Value < DateTimeOffset.Now)
                throw new InvalidOperationException("Token has expired");

            return token;
        }
    }

    public Task SetAsync(string? token, DateTimeOffset? expiration = null)
    {
        var subKey = Registry.CurrentUser.OpenSubKey("Software\\VkNet.Extensions.Auth", true) ?? 
                     Registry.CurrentUser.CreateSubKey("Software\\VkNet.Extensions.Auth", true);
            
        if (token is null)
            subKey.DeleteValue("Token");
        else
            subKey.SetValue("Token", token, RegistryValueKind.String);
        
        _token = token;
        Expiration = expiration;
        return Task.CompletedTask;
    }
}