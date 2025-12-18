using VkNet.Abstractions;
using VkNet.Abstractions.Authorization;
using VkNet.Extensions.DependencyInjection.Abstractions;
using VkNet.Model;

namespace VkNet.Extensions.DependencyInjection.Services;

internal sealed class VkApiAuth(
    IAuthorizationFlow authorizationFlow,
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

    public void RefreshToken(Task<string>? code = null)
    {
        RefreshTokenAsync(code).GetAwaiter().GetResult();
    }

    public void LogOut()
    {
        LogOutAsync().GetAwaiter().GetResult();
    }

    public bool IsAuthorized { get; private set;}
    public async Task AuthorizeAsync(IApiAuthParams @params, CancellationToken token = default)
    {
        if (!string.IsNullOrEmpty(@params.AccessToken))
        {
            await tokenStore.SetAsync(@params.AccessToken);
            return;
        }
        
        authorizationFlow.SetAuthorizationParams(@params);

        var result = await authorizationFlow.AuthorizeAsync(token);
        
        await tokenStore.SetAsync(result.AccessToken,
                                   result.ExpiresIn > 0
                                       ? DateTimeOffset.Now + TimeSpan.FromSeconds(result.ExpiresIn)
                                       : null);

        _lastAuthParams = @params;
    }

    public Task RefreshTokenAsync(Func<string>? code = null, CancellationToken token = default)
    {
        if (_lastAuthParams is null || !_lastAuthParams.IsValid)
        {
            throw new InvalidOperationException();
        }

        if (code is not null)
            _lastAuthParams.TwoFactorAuthorization = code;

        return tokenRefreshHandler?.RefreshTokenAsync(tokenStore.Token) ?? AuthorizeAsync(_lastAuthParams, token);
    }

    public Task RefreshTokenAsync(Task<string>? code = null, CancellationToken token = default)
    {
        if (_lastAuthParams is null || !_lastAuthParams.IsValid)
        {
            throw new InvalidOperationException();
        }

        if (code is not null)
            _lastAuthParams.TwoFactorAuthorizationAsync = code;
        
        return tokenRefreshHandler?.RefreshTokenAsync(tokenStore.Token) ?? AuthorizeAsync(_lastAuthParams, token);
    }

    public Task LogOutAsync(CancellationToken token = default)
    {
        IsAuthorized = false;
        return tokenStore.SetAsync(null);
    }
}