namespace WpfApp.ViewModels;

public class BrowserCaptchaViewModel : ViewModelBase
{
    private TaskCompletionSource<string?>? _solveResult;
    
    public Uri? CaptchaUri { get; set; }
    
    public ValueTask<string?> SolveAsync()
    {
        _solveResult = new();
        return new ValueTask<string?>(_solveResult.Task);
    }

    public void OnCaptchaSolved(string result)
    {
        _solveResult?.TrySetResult(result);
    }
}