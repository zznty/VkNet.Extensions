using Wpf.Ui.Controls;
using WpfApp.ViewModels;

namespace WpfApp.Views;

public partial class LoginPage : INavigableView<LoginViewModel>
{
    public LoginPage(LoginViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = this;
        
        InitializeComponent();
    }

    public LoginViewModel ViewModel { get; }
}