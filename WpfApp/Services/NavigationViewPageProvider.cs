using Wpf.Ui.Abstractions;

namespace WpfApp.Services;

public class NavigationViewPageProvider(IServiceProvider provider) : INavigationViewPageProvider
{
    public object? GetPage(Type pageType) => provider.GetService(pageType);
}