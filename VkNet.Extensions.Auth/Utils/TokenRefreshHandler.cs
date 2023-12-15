using Microsoft.Extensions.DependencyInjection;
using VkNet.Abstractions;
using VkNet.Extensions.Auth.Abstractions;
using VkNet.Extensions.Auth.Models.Auth;
using VkNet.Extensions.DependencyInjection.Abstractions;
using IAuthCategory = VkNet.Extensions.Auth.Abstractions.Categories.IAuthCategory;

namespace VkNet.Extensions.Auth.Utils;

public class TokenRefreshHandler(
    IExchangeTokenStore exchangeTokenStore,
    IServiceProvider serviceProvider,
    IVkTokenStore tokenStore)
    : ITokenRefreshHandler
{
    public async Task<string?> RefreshTokenAsync(string oldToken)
    {
        if (oldToken.StartsWith("anonym"))
        {
            var auth = serviceProvider.GetRequiredService<IVkApiAuthAsync>();
            
            await auth.AuthorizeAsync(new AndroidApiAuthParams());
            
            return tokenStore.Token;
        }
        
        if (await exchangeTokenStore.GetExchangeTokenAsync() is not { } exchangeToken)
            return null;

        var authCategory = serviceProvider.GetRequiredService<IAuthCategory>();

        var tokenInfo = await authCategory.RefreshTokensAsync(oldToken, exchangeToken);
        
        if (tokenInfo is null)
            return null;

        var (token, expiresIn) = tokenInfo;

        await tokenStore.SetAsync(token, DateTimeOffset.Now + TimeSpan.FromSeconds(expiresIn));

        var (newExchangeToken, _) = await authCategory.GetExchangeToken();
        
        await exchangeTokenStore.SetExchangeTokenAsync(newExchangeToken);

        return token;
    }
}