using Wpf.Ui.Abstractions.Controls;
using WpfApp.ViewModels;

namespace WpfApp.Views;

public partial class PasswordPage : INavigableView<PasswordViewModel>
{
    public PasswordPage(PasswordViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = this;
        
        InitializeComponent();
    }

    public PasswordViewModel ViewModel { get; }
}