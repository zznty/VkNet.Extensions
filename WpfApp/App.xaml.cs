using System.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using VkNet.Extensions.Auth.Abstractions;
using VkNet.Extensions.Auth.Extensions;
using VkNet.Extensions.DependencyInjection.Abstractions;
using Wpf.Ui;
using Wpf.Ui.Abstractions;
using WpfApp.Abstractions;
using WpfApp.Services;
using WpfApp.Services.Stores;
using WpfApp.ViewModels;
using WpfApp.Views;

namespace WpfApp;

public partial class App
{
    private readonly IHost _host = Host.CreateDefaultBuilder()
        .ConfigureAppConfiguration(c =>
        {
            c.SetBasePath(AppContext.BaseDirectory);
        })
        .ConfigureServices(services =>
        {
            // Vk Net implementations
            services.AddSingleton<IVkTokenStore, RegistryTokenStore>();
            services.AddSingleton<IDeviceIdProvider, WindowsDeviceIdProvider>();
            services.AddSingleton<IExchangeTokenStore, RegistryExchangeTokenStore>();
            services.AddSingleton<IAsyncCaptchaSolver, WpfAppCaptchaSolver>();
            
            // Vk Net services
            services.AddVkNetWithAuth();

            // Wpf Ui services
            services.AddSingleton<ISnackbarService, SnackbarService>();
            services.AddSingleton<INavigationService, NavigationService>();
            services.AddSingleton<IContentDialogService, ContentDialogService>();
            services.AddSingleton<INavigationViewPageProvider, NavigationViewPageProvider>();
            
            // Wpf app services
            services.AddSingleton<TokenChecker>();
            
            // Main window
            services.AddSingleton<IWindow, MainWindow>();
            services.AddSingleton<MainWindowViewModel>();
            
            // Pages
            services.AddTransient<LoginPage>();
            services.AddTransient<PasswordPage>();
            services.AddTransient<PasskeyPage>();
            services.AddTransient<OtpCodePage>();
            services.AddTransient<UserInfoPage>();
            services.AddTransient<BrowserCaptchaPage>();
            
            // View models
            services.AddScoped<LoginViewModel>();
            services.AddScoped<PasswordViewModel>();
            services.AddScoped<OtpCodeViewModel>();
            services.AddSingleton<BrowserCaptchaViewModel>();
            services.AddTransient<UserInfoViewModel>();

            services.AddHostedService<ApplicationHostService>();
        })
        .Build();
    
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        
        _host.Start();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        base.OnExit(e);

        _host.StopAsync().Wait();
        _host.Dispose();
    }
}