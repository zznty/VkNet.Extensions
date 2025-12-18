namespace VkNet.Extensions.Auth.Abstractions.Interop;

public interface IPlatformPasskeyApi
{
    Task<string> RequestPasskeyAsync(string passkeyData, string origin);
}