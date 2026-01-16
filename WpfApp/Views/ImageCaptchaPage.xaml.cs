using WpfApp.ViewModels;

namespace WpfApp.Views;

public partial class ImageCaptchaPage
{
    public ImageCaptchaPage(ImageCaptchaViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = this;

        InitializeComponent();

        Loaded += (s, e) => CaptchaTextBox.Focus();
    }

    public ImageCaptchaViewModel ViewModel { get; }
}
