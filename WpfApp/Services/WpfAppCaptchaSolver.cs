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
        if (request is BrowserCaptchaRequest { RedirectUri: var uri })
        {
            var viewModel = provider.GetRequiredService<BrowserCaptchaViewModel>();

            viewModel.CaptchaUri = uri;

            navigationService.NavigateWithHierarchy(typeof(BrowserCaptchaPage));

            var result = await viewModel.SolveAsync();

            navigationService.GoBack();

            return result;
        }

        if (request is ImageCaptchaRequest { ImageUri: var imgUri })
        {
            // Для image captcha используем специальный диалог
            var viewModel = provider.GetRequiredService<ImageCaptchaViewModel>();

            viewModel.CaptchaUri = imgUri;

            navigationService.NavigateWithHierarchy(typeof(ImageCaptchaPage));

            var result = await viewModel.SolveAsync();

            navigationService.GoBack();

            return result;
        }

        return null;
    }

    public ValueTask SolveFailedAsync()
    {
        return ValueTask.CompletedTask;
    }
}