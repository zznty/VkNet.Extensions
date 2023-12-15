namespace VkNet.Extensions.Auth.Abstractions.Categories;

public interface ILoginCategory
{
    Task ConnectAsync(string uuid, CancellationToken token = default);
    Task ConnectAuthCodeAsync(string token, string uuid, CancellationToken cancellationToken = default);
}