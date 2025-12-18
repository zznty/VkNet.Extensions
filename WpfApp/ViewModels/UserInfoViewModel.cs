using VkNet.Abstractions;
using VkNet.Enums.Filters;
using VkNet.Model;
using Wpf.Ui.Abstractions.Controls;

namespace WpfApp.ViewModels;

public class UserInfoViewModel(IUsersCategory category) : ViewModelBase, INavigationAware
{
    public User? CurrentUser { get; private set; }
    
    public async Task OnNavigatedToAsync()
    {
        var response = await category.GetAsync(Enumerable.Empty<long>(), ProfileFields.FirstName | ProfileFields.LastName | ProfileFields.Photo200);
        
        CurrentUser = response.FirstOrDefault();
    }

    public Task OnNavigatedFromAsync()
    {
        CurrentUser = null;
        return Task.CompletedTask;
    }
}