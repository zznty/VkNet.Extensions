using VkNet.Abstractions;
using VkNet.Enums.Filters;
using VkNet.Model;
using Wpf.Ui.Controls;

namespace WpfApp.ViewModels;

public class UserInfoViewModel(IUsersCategory category) : ViewModelBase, INavigationAware
{
    public User? CurrentUser { get; private set; }
    
    public async void OnNavigatedTo()
    {
        var response = await category.GetAsync(Enumerable.Empty<long>(), ProfileFields.FirstName | ProfileFields.LastName | ProfileFields.Photo200);
        
        CurrentUser = response.FirstOrDefault();
    }

    public void OnNavigatedFrom()
    {
        CurrentUser = null;
    }
}