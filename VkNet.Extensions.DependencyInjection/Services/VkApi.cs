using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VkNet.Abstractions;
using VkNet.Abstractions.Authorization;
using VkNet.Abstractions.Category;
using VkNet.Abstractions.Core;
using VkNet.Enums;
using VkNet.Extensions.DependencyInjection.Abstractions;
using VkNet.Model;
using VkNet.Utils;
using VkNet.Utils.AntiCaptcha;

namespace VkNet.Extensions.DependencyInjection.Services;

public class VkApi(
    IVkApiAuthAsync auth,
    IVkApiInvoke invoke,
    ILanguageService languageService,
    IVkApiCategories categories,
    IAuthorizationFlow authorizationFlow,
    INeedValidationHandler needValidationHandler,
    IVkApiVersionManager vkApiVersion,
    IVkTokenStore tokenStore,
    ICaptchaHandler captchaHandler,
    ICaptchaSolver? captchaSolver = null)
    : IVkApi
{
    public void Dispose()
    {
    }

    public void Authorize(IApiAuthParams @params) =>
        auth.Authorize(@params);

    public void Authorize(ApiAuthParams @params) =>
        auth.Authorize(@params);

    public void RefreshToken(Func<string>? code = null) =>
        auth.RefreshToken(code);

    public void LogOut() => auth.LogOut();

    public bool IsAuthorized => auth.IsAuthorized;

    public Task AuthorizeAsync(IApiAuthParams @params) =>
        auth.AuthorizeAsync(@params);

    public Task RefreshTokenAsync(Func<string>? code = null) =>
        auth.RefreshTokenAsync(code);

    public Task LogOutAsync() => auth.LogOutAsync();

    public IUsersCategory Users => categories.Users;
    public IFriendsCategory Friends => categories.Friends;
    public IStatusCategory Status => categories.Status;
    public IMessagesCategory Messages => categories.Messages;
    public IGroupsCategory Groups => categories.Groups;
    public IAudioCategory Audio =>  categories.Audio;
    public IDatabaseCategory Database => categories.Database;
    public IUtilsCategory Utils => categories.Utils;
    public IWallCategory Wall => categories.Wall;
    public IBoardCategory Board => categories.Board;
    public IFaveCategory Fave => categories.Fave;
    public IVideoCategory Video => categories.Video;
    public IAccountCategory Account => categories.Account;
    public IPhotoCategory Photo => categories.Photo;
    public IDocsCategory Docs => categories.Docs;
    public ILikesCategory Likes => categories.Likes;
    public IPagesCategory Pages => categories.Pages;
    public IAppsCategory Apps => categories.Apps;
    public INewsFeedCategory NewsFeed => categories.NewsFeed;
    public IStatsCategory Stats => categories.Stats;
    public IGiftsCategory Gifts => categories.Gifts;
    public IMarketsCategory Markets => categories.Markets;
    public IAuthCategory Auth => categories.Auth;
    public IExecuteCategory Execute => categories.Execute;
    public IPollsCategory PollsCategory => categories.PollsCategory;
    public ISearchCategory Search => categories.Search;
    public IStorageCategory Storage => categories.Storage;
    public IAdsCategory Ads => categories.Ads;
    public INotificationsCategory Notifications => categories.Notifications;
    public IWidgetsCategory Widgets => categories.Widgets;
    public ILeadsCategory Leads => categories.Leads;
    public IStreamingCategory Streaming => categories.Streaming;
    public IPlacesCategory Places => categories.Places;
    public INotesCategory Notes { get => categories.Notes; set => throw new NotSupportedException(); }
    public IAppWidgetsCategory AppWidgets { get => categories.AppWidgets; set => throw new NotSupportedException(); }
    public IOrdersCategory Orders { get => categories.Orders; set => throw new NotSupportedException(); }
    public ISecureCategory Secure { get => categories.Secure; set => throw new NotSupportedException(); }
    public IStoriesCategory Stories { get => categories.Stories; set => throw new NotSupportedException(); }
    public ILeadFormsCategory LeadForms { get => categories.LeadForms; set => throw new NotSupportedException(); }
    public IPrettyCardsCategory PrettyCards { get => categories.PrettyCards; set => throw new NotSupportedException(); }
    public IPodcastsCategory Podcasts { get => categories.Podcasts; set => throw new NotSupportedException(); }
    public IDonutCategory Donut => categories.Donut;
    public IDownloadedGamesCategory DownloadedGames => categories.DownloadedGames;
    public ICaptchaSolver? CaptchaSolver { get; } = captchaSolver;

    public int MaxCaptchaRecognitionCount
    {
        get => captchaHandler.MaxCaptchaRecognitionCount;
        set => captchaHandler.MaxCaptchaRecognitionCount = value;
    }

    public VkResponse Call(string methodName, VkParameters parameters, bool skipAuthorization = false) =>
        invoke.Call(methodName, parameters, skipAuthorization);

    public T Call<T>(string methodName, VkParameters parameters, bool skipAuthorization = false,
                     params JsonConverter[] jsonConverters) =>
        invoke.Call<T>(methodName, parameters, skipAuthorization, jsonConverters);

    public Task<VkResponse> CallAsync(string methodName, VkParameters parameters, bool skipAuthorization = false) =>
        invoke.CallAsync(methodName, parameters, skipAuthorization);

    public Task<T> CallAsync<T>(string methodName, VkParameters parameters, bool skipAuthorization = false) =>
        invoke.CallAsync<T>(methodName, parameters, skipAuthorization);

    public string Invoke(string methodName, IDictionary<string, string> parameters, bool skipAuthorization = false) =>
        invoke.Invoke(methodName, parameters, skipAuthorization);

    public Task<string> InvokeAsync(string methodName, IDictionary<string, string> parameters,
                                    bool skipAuthorization = false) =>
        invoke.InvokeAsync(methodName, parameters, skipAuthorization);

    public DateTimeOffset? LastInvokeTime => invoke.LastInvokeTime;
    public TimeSpan? LastInvokeTimeSpan => invoke.LastInvokeTimeSpan;
    
    public VkResponse CallLongPoll(string server, VkParameters parameters)
    {
        throw new NotImplementedException();
    }

    public Task<VkResponse> CallLongPollAsync(string server, VkParameters parameters)
    {
        throw new NotImplementedException();
    }

    public string InvokeLongPoll(string server, Dictionary<string, string> parameters)
    {
        throw new NotImplementedException();
    }

    public JObject InvokeLongPollExtended(string server, Dictionary<string, string> parameters)
    {
        throw new NotImplementedException();
    }

    public Task<string> InvokeLongPollAsync(string server, Dictionary<string, string> parameters)
    {
        throw new NotImplementedException();
    }

    public Task<JObject> InvokeLongPollExtendedAsync(string server, Dictionary<string, string> parameters)
    {
        throw new NotImplementedException();
    }

    public void SetLanguage(Language language) =>
        languageService.SetLanguage(language);

    public Language? GetLanguage() =>
        languageService.GetLanguage();

    public void Validate(string validateUrl) =>
        needValidationHandler.ValidateAsync(validateUrl).GetAwaiter().GetResult();

    public int RequestsPerSecond { get; set; }

    [Obsolete]
    public IBrowser Browser
    {
        get => throw new NotImplementedException();
        set => throw new NotImplementedException();
    }

    public IAuthorizationFlow AuthorizationFlow
    {
        get => authorizationFlow;
        set => throw new NotSupportedException("Container is immutable");
    }

    public INeedValidationHandler NeedValidationHandler
    {
        get => needValidationHandler;
        set => throw new NotSupportedException("Container is immutable");
    }

    public IVkApiVersionManager VkApiVersion
    {
        get => vkApiVersion;
        set => throw new NotSupportedException("Container is immutable");
    }

    public string Token => tokenStore.Token;
    public long? UserId { get; set; }
    public event VkApiDelegate? OnTokenExpires
    {
        add => throw new NotImplementedException();
        remove => throw new NotImplementedException();
    }

    public event VkApiDelegate? OnTokenUpdatedAutomatically
    {
        add => throw new NotImplementedException();
        remove => throw new NotImplementedException();
    }
}