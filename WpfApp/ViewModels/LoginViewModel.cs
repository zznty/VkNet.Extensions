using System.Windows.Input;
using AsyncAwaitBestPractices.MVVM;
using Microsoft.Extensions.DependencyInjection;
using VkNet.Abstractions;
using VkNet.Extensions.Auth.Models.Auth;
using Wpf.Ui;
using WpfApp.Views;

namespace WpfApp.ViewModels;

public class LoginViewModel : ViewModelBase
{
    private readonly IVkApiAuthAsync _auth;
    private readonly INavigationService _navigationService;
    private readonly IServiceProvider _serviceProvider;
    public ICommand LoginCommand { get; }
    
    public TaskCompletionSource<bool> LoggedIn { get; } = new();
    
    public LoginViewModel(IVkApiAuthAsync auth, INavigationService navigationService, IServiceProvider serviceProvider)
    {
        _auth = auth;
        _navigationService = navigationService;
        _serviceProvider = serviceProvider;
        LoginCommand = new AsyncCommand<string>(LoginAsync);
    }

    private async Task LoginAsync(string? login)
    {
        if (string.IsNullOrEmpty(login))
            return;

        await _auth.AuthorizeAsync(new AndroidApiAuthParams());
        
        await _auth.AuthorizeAsync(new AndroidApiAuthParams(login, null, CodeRequestedAsync));
        
        LoggedIn.SetResult(true);
    }

    private ValueTask<string?> CodeRequestedAsync(LoginWay requestedLoginWay, AuthState state)
    {
        if (requestedLoginWay == LoginWay.Passkey)
        {
            _navigationService.Navigate(typeof(PasskeyPage));
            return default;
        }

        if (requestedLoginWay == LoginWay.Password)
        {
            var passwordViewModel = _serviceProvider.GetRequiredService<PasswordViewModel>();
            
            if (state is ProfileAuthState profileAuthState)
                passwordViewModel.Profile = profileAuthState.Profile;
            
            _navigationService.Navigate(typeof(PasswordPage));

            return new(passwordViewModel.PasswordSubmitted.Task);
        }

        requestedLoginWay = UnwrapTwoFactorWay(requestedLoginWay);
        
        var otpCodeViewModel = _serviceProvider.GetRequiredService<OtpCodeViewModel>();
        
        otpCodeViewModel.LoginWay = requestedLoginWay;

        if (state is VerificationAuthState verificationAuthState)
        {
            otpCodeViewModel.CodeLength = verificationAuthState.CodeLength;
            otpCodeViewModel.Info = verificationAuthState.Info;
        }
        
        _navigationService.Navigate(typeof(OtpCodePage));
        
        return new(otpCodeViewModel.Submitted.Task);
    }

    private static LoginWay UnwrapTwoFactorWay(LoginWay way)
    {
        if (way == LoginWay.TwoFactorCallReset)
            return LoginWay.CallReset;
        if (way == LoginWay.TwoFactorSms)
            return LoginWay.Sms;
        if (way == LoginWay.TwoFactorPush)
            return LoginWay.Push;
        if (way == LoginWay.TwoFactorEmail)
            return LoginWay.Email;
        
        return way;
    }
}