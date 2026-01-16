using System.Windows.Input;
using AsyncAwaitBestPractices.MVVM;

namespace WpfApp.ViewModels;

public class ImageCaptchaViewModel : ViewModelBase
{
    private TaskCompletionSource<string?>? _solveResult;
    private string _captchaCode = string.Empty;

    public Uri? CaptchaUri { get; set; }

    public string CaptchaCode
    {
        get => _captchaCode;
        set
        {
            _captchaCode = value;
            OnPropertyChanged();
        }
    }

    public ICommand SubmitCommand { get; }

    public ImageCaptchaViewModel()
    {
        SubmitCommand = new AsyncCommand(Submit);
    }

    public ValueTask<string?> SolveAsync()
    {
        _solveResult = new();
        return new ValueTask<string?>(_solveResult.Task);
    }

    private Task Submit()
    {
        _solveResult?.TrySetResult(CaptchaCode);
        return Task.CompletedTask;
    }
}
