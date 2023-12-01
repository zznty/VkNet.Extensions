namespace VkNet.Extensions.Auth.Abstractions.Categories;

public interface ILoginCategory
{
    Task ConnectAsync(string uuid);
    Task ConnectAuthCodeAsync(string token, string uuid);
}