using VkNet.Abstractions;
using VkNet.Abstractions.Authorization;
using VkNet.Extensions.Auth.Models.Auth;
using VkNet.Extensions.DependencyInjection.Abstractions;
using VkNet.Model;

namespace VkNet.Extensions.Auth.Utils;

public class VkApiAuth(
    IAuthorizationFlow flow,
    IVkTokenStore tokenStore,
    ITokenRefreshHandler? tokenRefreshHandler = null)
    : IVkApiAuthAsync
{
    private IApiAuthParams? _lastAuthParams;

    public void Authorize(IApiAuthParams @params)
    {
        AuthorizeAsync(@params).GetAwaiter().GetResult();
    }

    public void Authorize(ApiAuthParams @params)
    {
        Authorize((IApiAuthParams)@params);
    }

    public void RefreshToken(Func<string>? code = null)
    {
        RefreshTokenAsync(code).GetAwaiter().GetResult();
    }

    public void LogOut()
    {
        LogOutAsync().GetAwaiter().GetResult();
    }

    public bool IsAuthorized { get; private set;}
    public async Task AuthorizeAsync(IApiAuthParams @params)
    {
        if (!string.IsNullOrEmpty(@params.AccessToken))
        {
            await tokenStore.SetAsync(@params.AccessToken);
            return;
        }
        
        if (@params is not AndroidApiAuthParams)
            throw new ArgumentException("Invalid auth params", nameof(@params));
        
        flow.SetAuthorizationParams(@params);

        var result = await flow.AuthorizeAsync();
        
        await tokenStore.SetAsync(result.AccessToken,
                                   result.ExpiresIn > 0
                                       ? DateTimeOffset.Now + TimeSpan.FromSeconds(result.ExpiresIn)
                                       : null);

        _lastAuthParams = @params;
    }

    public Task RefreshTokenAsync(Func<string>? code = null)
    {
        if (_lastAuthParams is null || !_lastAuthParams.IsValid)
        {
            throw new InvalidOperationException();
        }

        if (code is not null)
            _lastAuthParams.TwoFactorAuthorization = code;

        return tokenRefreshHandler?.RefreshTokenAsync(tokenStore.Token) ?? AuthorizeAsync(_lastAuthParams);
    }

    public Task LogOutAsync()
    {
        IsAuthorized = false;
        return tokenStore.SetAsync(null);
    }
}