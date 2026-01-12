using System.Collections.Immutable;

namespace VkNet.Extensions.Auth.Utils;

public record VkAuthRegisterOptions(string UserAgent, bool UseHttp3, ImmutableDictionary<string, string> AdditionalHeaders)
{
    public static VkAuthRegisterOptions DefaultAndroid { get; } =
        new("VKAndroidApp/8.157-45261 (Android 14; SDK 34; arm64-v8a; Pixel 8; ru; 2220x1080)", true,
            [new("X-VK-Android-Client", "new")]);

    public virtual void ConfigureClient(HttpClient client)
    {
        client.BaseAddress = new("https://api.vk.com/method/");
        client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", UserAgent);
        if (UseHttp3)
            client.DefaultRequestHeaders.TryAddWithoutValidation("X-Quic", "1");
        
        foreach (var (key, value) in AdditionalHeaders)
        {
            client.DefaultRequestHeaders.TryAddWithoutValidation(key, value);
        }
    }
}