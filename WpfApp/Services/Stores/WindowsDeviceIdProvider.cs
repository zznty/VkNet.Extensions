using System.Runtime.InteropServices.WindowsRuntime;
using Windows.System.Profile;
using VkNet.Extensions.Auth.Abstractions;

namespace WpfApp.Services.Stores;

public class WindowsDeviceIdProvider : IDeviceIdProvider
{
    private readonly string _id = Convert.ToHexStringLower(SystemIdentification.GetSystemIdForPublisher().Id.ToArray());

    public ValueTask<string> GetDeviceIdAsync() => ValueTask.FromResult(_id);
}