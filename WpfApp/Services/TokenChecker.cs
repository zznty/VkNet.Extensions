using VkNet.Abstractions;
using VkNet.Exception;

namespace WpfApp.Services;

public class TokenChecker(IUsersCategory usersCategory)
{
    public async Task<bool> IsTokenValid()
    {
        try
        {
            await usersCategory.GetAsync(Enumerable.Empty<long>());
        }
        catch (Exception e) when (e is VkApiException or InvalidOperationException)
        {
            return false;
        }

        return true;
    }
}