using Wpf.Ui.Controls;
using WpfApp.ViewModels;

namespace WpfApp.Views;

public partial class UserInfoPage : INavigableView<UserInfoViewModel>
{
    public UserInfoPage(UserInfoViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = this;
        
        InitializeComponent();
    }

    public UserInfoViewModel ViewModel { get; }
}