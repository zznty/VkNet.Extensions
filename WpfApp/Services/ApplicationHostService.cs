using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using VkNet.Abstractions.Core;
using Wpf.Ui;
using WpfApp.Abstractions;
using WpfApp.ViewModels;
using WpfApp.Views;

namespace WpfApp.Services;

/// <summary>
/// Managed host of the application.
/// </summary>
public class ApplicationHostService(IServiceProvider serviceProvider, TokenChecker tokenChecker, INavigationService navigationService, IServiceScopeFactory serviceScopeFactory, IVkApiVersionManager versionManager) : IHostedService
{
    /// <summary>
    /// Triggered when the application host is ready to start the service.
    /// </summary>
    /// <param name="cancellationToken">Indicates that the start process has been aborted.</param>
    public Task StartAsync(CancellationToken cancellationToken)
    {
        versionManager.SetVersion(5, 220);
        
        return HandleActivationAsync();
    }

    /// <summary>
    /// Triggered when the application host is performing a graceful shutdown.
    /// </summary>
    /// <param name="cancellationToken">Indicates that the shutdown process should no longer be graceful.</param>
    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Creates main window during activation.
    /// </summary>
    private Task HandleActivationAsync()
    {
        if (Application.Current.Windows.OfType<MainWindow>().Any())
        {
            return Task.CompletedTask;
        }

        var mainWindow = serviceProvider.GetRequiredService<IWindow>();
        mainWindow.Loaded += OnMainWindowLoaded;
        mainWindow.Show();

        return Task.CompletedTask;
    }

    private async void OnMainWindowLoaded(object sender, RoutedEventArgs e)
    {
        if (await tokenChecker.IsTokenValid())
        {
            navigationService.Navigate(typeof(UserInfoPage));
            return;
        }

        using var scope = serviceScopeFactory.CreateScope();
        
        navigationService.GetNavigationControl().SetServiceProvider(scope.ServiceProvider);
        
        if (!navigationService.Navigate(typeof(LoginPage)))
            return;
        
        var loginViewModel = scope.ServiceProvider.GetRequiredService<LoginViewModel>();
        var loginResult = await loginViewModel.LoggedIn.Task;
        
        navigationService.GetNavigationControl().SetServiceProvider(serviceProvider);
        if (loginResult)
            navigationService.Navigate(typeof(UserInfoPage));
    }
}