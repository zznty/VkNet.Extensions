using Microsoft.Extensions.DependencyInjection;
using VkNet.Extensions.DependencyInjection.Abstractions;
using Wpf.Ui;
using WpfApp.ViewModels;
using WpfApp.Views;

namespace WpfApp.Services;

public class WpfAppCaptchaSolver(INavigationService navigationService, IServiceProvider provider) : IAsyncCaptchaSolver
{
    public async ValueTask<string?> SolveAsync(CaptchaRequest request)
    {
        if (request is not BrowserCaptchaRequest { RedirectUri: var uri })
            return null;
        
        var viewModel = provider.GetRequiredService<BrowserCaptchaViewModel>();

        viewModel.CaptchaUri = uri;
        
        navigationService.NavigateWithHierarchy(typeof(BrowserCaptchaPage));

        var result = await viewModel.SolveAsync();

        navigationService.GoBack();
        
        return result;
    }

    public ValueTask SolveFailedAsync()
    {
        return ValueTask.CompletedTask;
    }
}