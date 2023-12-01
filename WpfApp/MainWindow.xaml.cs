using Wpf.Ui;
using WpfApp.Abstractions;

namespace WpfApp;

public partial class MainWindow : IWindow
{
    public MainWindow(INavigationService navigationService, ISnackbarService snackbarService,
        IContentDialogService contentDialogService, IServiceProvider serviceProvider)
    {
        InitializeComponent();
        
        snackbarService.SetSnackbarPresenter(SnackbarPresenter);
        navigationService.SetNavigationControl(NavigationView);
        contentDialogService.SetContentPresenter(RootContentDialog);
        
        NavigationView.SetServiceProvider(serviceProvider);
    }
}