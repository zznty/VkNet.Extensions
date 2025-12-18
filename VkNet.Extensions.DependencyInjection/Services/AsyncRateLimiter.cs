using VkNet.Extensions.DependencyInjection.Abstractions;

namespace VkNet.Extensions.DependencyInjection.Services;

public class AsyncRateLimiter(TimeSpan window, int maxRequestsPerWindow) : IAsyncRateLimiter
{
    private readonly SemaphoreSlim _semaphore = new(maxRequestsPerWindow);
    private readonly CancellationTokenSource _tokenSource = new();

    public TimeSpan Window { get; } = window;
    public int MaxRequestsPerWindow { get; } = maxRequestsPerWindow;

    public async ValueTask WaitNextAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (TryGetNext())
                return;

            var source = cancellationToken != default
                ? CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _tokenSource.Token)
                : null;

            await _semaphore.WaitAsync(source?.Token ?? cancellationToken);

            source?.Dispose();
        }
        finally
        {
            ReleaseAsync();
        }
    }

    private async void ReleaseAsync()
    {
        await Task.Delay(Window, _tokenSource.Token).ConfigureAwait(false);
        _semaphore.Release();
    }

    public ValueTask<bool> WaitNextAsync(int timeout) => WaitNextAsync(TimeSpan.FromMilliseconds(timeout));

    public async ValueTask<bool> WaitNextAsync(TimeSpan timeout)
    {
        try
        {
            using var source = new CancellationTokenSource(timeout);
            await WaitNextAsync(source.Token);
        }
        catch (OperationCanceledException)
        {
            return false;
        }

        return true;
    }

    public bool TryGetNext()
    {
        if (_semaphore.CurrentCount == 0) return false;
        
        _semaphore.Wait();
        return true;
    }

    public void Dispose()
    {
        _tokenSource.Cancel();
        _tokenSource.Dispose();
        _semaphore.Dispose();
    }
}