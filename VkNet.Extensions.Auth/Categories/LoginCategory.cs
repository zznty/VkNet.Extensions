using System.Net.Http.Json;
using System.Security.Authentication;
using System.Text.Json;
using VkNet.Extensions.Auth.Abstractions.Categories;
using VkNet.Extensions.DependencyInjection.Abstractions;
using VkNet.Utils;

namespace VkNet.Extensions.Auth.Categories;

public class LoginCategory(IVkTokenStore tokenStore, HttpClient client) : ILoginCategory
{
    private readonly JsonSerializerOptions _jsonSerializerOptions = new(JsonSerializerDefaults.Web);

    public async Task ConnectAsync(string uuid, CancellationToken token = default)
    {
        var parameters = new VkParameters
        {
            { "uuid", uuid },
            { "version", 1 },
            { "app_id", 7913379 }
        };
        
        using var response = await client.PostAsync("https://login.vk.com/?act=connect_internal", new FormUrlEncodedContent(parameters), token);

        var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>(_jsonSerializerOptions, cancellationToken: token);

        if (loginResponse?.Type != "okay")
            throw new AuthenticationException();
    }

    public async Task ConnectAuthCodeAsync(string token, string uuid, CancellationToken cancellationToken = default)
    {
        var parameters = new VkParameters
        {
            { "uuid", uuid },
            { "version", 1 },
            { "app_id", 7913379 },
            { "token", token }
        };
        
        var response = await client.PostAsync("https://login.vk.com/?act=connect_code_auth", new FormUrlEncodedContent(parameters), cancellationToken);

        var (type, loginResponse) = (await response.Content.ReadFromJsonAsync<LoginResponse<LoginResponseAccessToken>>(_jsonSerializerOptions, cancellationToken: cancellationToken))!;

        if (type != "okay" || loginResponse.ResponseType != "auth_token")
            throw new AuthenticationException();

        await tokenStore.SetAsync(loginResponse.AccessToken);
    }
}

public record LoginResponse(string Type);
public record LoginResponse<TData>(string Type, TData Data) : LoginResponse(Type) where TData : class;

public record LoginResponseAccessToken(string ResponseType, string AccessToken);