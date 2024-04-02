using Microsoft.Extensions.DependencyInjection;
using VkNet.Abstractions.Authorization;
using VkNet.Extensions.Auth.Abstractions.Categories;
using VkNet.Extensions.Auth.Abstractions.Interop;
using VkNet.Extensions.Auth.Exceptions;
using VkNet.Extensions.Auth.Models.Auth;
using VkNet.Extensions.Auth.Models.Ecosystem;
using VkNet.Extensions.DependencyInjection.Abstractions;
using VkNet.Model;

namespace VkNet.Extensions.Auth.Flows;

public class VkAndroidAuthorizationFlow(
    IServiceProvider serviceProvider,
    IVkTokenStore tokenStore,
    IAuthCategory authCategory,
    IEcosystemCategory ecosystemCategory,
    IPlatformPasskeyApi? platformPasskeyApi = null) : IAuthorizationFlow
{
    private AndroidApiAuthParams? _apiAuthParams;

    public async Task<AuthorizationResult> AuthorizeAsync(CancellationToken token = default)
    {
        if (_apiAuthParams is null)
            throw new InvalidOperationException($"Authorization parameters are not set. Call {nameof(SetAuthorizationParams)} first.");
        
        if (_apiAuthParams.IsAnonymous)
        {
            var defaultFlow = serviceProvider.GetRequiredKeyedService<IAuthorizationFlow>(AndroidGrantType.Password);
            
            defaultFlow.SetAuthorizationParams(_apiAuthParams);
            
            return await defaultFlow.AuthorizeAsync(token);
        }
        
        EnsureAnonymousToken();
        
        if (_apiAuthParams.Login is null)
            throw new InvalidOperationException("Login is not set");

        var (_, _, _, _, sid, nextStep) =
            await authCategory.ValidateAccountAsync(_apiAuthParams.Login, passkeySupported: platformPasskeyApi is not null,
                loginWays: _apiAuthParams.SupportedWays, token: token);

        return await NextStepAsync(sid, nextStep?.VerificationMethod ?? LoginWay.Password, token: token);
    }

    private async Task<AuthorizationResult> NextStepAsync(string sid, LoginWay nextStep, EcosystemProfile? passwordProfile = null, CancellationToken token = default)
    {
        string? code = null;
        while (!_apiAuthParams!.CancellationToken.IsCancellationRequested)
        {
            if (nextStep == LoginWay.Passkey) return await AuthByPasskeyAsync(sid);

            if (nextStep == LoginWay.Password)
            {
                var passwordFlow = serviceProvider.GetRequiredKeyedService<IAuthorizationFlow>(passwordProfile is null
                    ? AndroidGrantType.Password
                    : AndroidGrantType.PhoneConfirmationSid);

                passwordFlow.SetAuthorizationParams(_apiAuthParams! with
                {
                    Sid = sid,
                    Code = code,
                    Password = await _apiAuthParams.CodeRequestedAsync!(LoginWay.Password,
                        passwordProfile is null ? new AuthState(sid) : new ProfileAuthState(sid, passwordProfile)),
                    SupportedWays = new[] { LoginWay.Push, LoginWay.Email }
                });

                return await passwordFlow.AuthorizeAsync(token);
            }

            var codeLength = 6;
            var info = _apiAuthParams!.Login!;

            if (nextStep == LoginWay.Sms)
            {
                var (_, otpSid, smsInfo, requestedCodeLength) = await ecosystemCategory.SendOtpSmsAsync(sid, token);

                sid = otpSid;
                codeLength = requestedCodeLength;
                info = smsInfo;
            }
            else if (nextStep == LoginWay.CallReset)
            {
                var (_, otpSid, smsInfo, requestedCodeLength) = await ecosystemCategory.SendOtpCallResetAsync(sid, token);

                sid = otpSid;
                codeLength = requestedCodeLength;
                info = smsInfo;
            }
            else if (nextStep == LoginWay.Push)
            {
                var (_, otpSid, smsInfo, requestedCodeLength) = await ecosystemCategory.SendOtpPushAsync(sid, token);

                sid = otpSid;
                codeLength = requestedCodeLength;
                info = smsInfo;
            }

            code = await _apiAuthParams.CodeRequestedAsync!(nextStep, new VerificationAuthState(sid, info, codeLength));

            if (code is null)
            {
                if (_apiAuthParams.VerificationMethodRequestedAsync == null)
                    throw new VkAuthException(new()
                    {
                        Error = "No more verification methods left",
                        ErrorDescription = "Verification choice handler is not defined and code was not provided",
                        ErrorType = "no_methods_left",
                    });

                var methods = await ecosystemCategory.GetVerificationMethodsAsync(sid, token);

                nextStep = await _apiAuthParams.VerificationMethodRequestedAsync(methods.Methods, new(sid));
                passwordProfile = null;
                continue;
            }

            var response = await ecosystemCategory.CheckOtpAsync(sid, nextStep, code, token);
            sid = response.Sid;

            if (!response.ProfileExist)
                throw new VkAuthException(new()
                {
                    Error = "Profile not found", ErrorDescription = "Profile not found",
                    ErrorType = "profile_not_found",
                });

            if (!response.CanSkipPassword)
            {
                nextStep = LoginWay.Password;
                passwordProfile = response.Profile;
                continue;
            }

            var flow = serviceProvider.GetRequiredKeyedService<IAuthorizationFlow>(AndroidGrantType.WithoutPassword);

            flow.SetAuthorizationParams(_apiAuthParams with
            {
                Sid = sid, Password = null, SupportedWays = new[] { LoginWay.Push, LoginWay.Email }
            });

            return await flow.AuthorizeAsync(token);
        }

        throw new(); // placeholder for compiler
    }

    private async Task<AuthorizationResult> AuthByPasskeyAsync(string sid)
    {
        if (platformPasskeyApi is null)
            throw new PlatformNotSupportedException("Current platform does not support passkey authorization");
        
        if (_apiAuthParams!.CodeRequestedAsync is not null)
            await _apiAuthParams.CodeRequestedAsync(LoginWay.Passkey, new(sid));
        
        var (_, passkeyData) = await authCategory.BeginPasskeyAsync(sid);

        var passkeyResponse = await platformPasskeyApi.RequestPasskeyAsync(passkeyData);

        var flow = serviceProvider.GetRequiredKeyedService<IAuthorizationFlow>(AndroidGrantType.Passkey);

        flow.SetAuthorizationParams(_apiAuthParams with
        {
            Sid = sid, 
            PasskeyData = passkeyResponse, 
            SupportedWays = new[] { LoginWay.Passkey }
        });
        
        return await flow.AuthorizeAsync();
    }

    private void EnsureAnonymousToken()
    {
        string token;
        try
        {
            token = tokenStore.Token;
        }
        catch (System.Exception e)
        {
            throw new InvalidOperationException("Failed to get token from store", e);
        }
        
        if (!token.StartsWith("anonym"))
            throw new InvalidOperationException("Token is not anonymous");
    }

    public void SetAuthorizationParams(IApiAuthParams authorizationParams)
    {
        _apiAuthParams = authorizationParams as AndroidApiAuthParams ?? throw new ArgumentException(
            $"Authorization parameters must be of type {nameof(AndroidApiAuthParams)}", nameof(authorizationParams)
        );
    }
}