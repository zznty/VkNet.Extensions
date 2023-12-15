using Microsoft.Extensions.DependencyInjection;
using VkNet.Abstractions;
using VkNet.Abstractions.Category;

namespace VkNet.Extensions.DependencyInjection.Services;

public class VkApiCategories(
    IUsersCategory users,
    IFriendsCategory friends,
    IStatusCategory status,
    IAudioCategory audio,
    IDatabaseCategory database,
    IUtilsCategory utils,
    IWallCategory wall,
    IBoardCategory board,
    IFaveCategory fave,
    IVideoCategory video,
    IAccountCategory account,
    IPhotoCategory photo,
    IDocsCategory docs,
    ILikesCategory likes,
    IPagesCategory pages,
    IAppsCategory apps,
    INewsFeedCategory newsFeed,
    IStatsCategory stats,
    IGiftsCategory gifts,
    IMarketsCategory markets,
    IAuthCategory auth,
    IPollsCategory pollsCategory,
    ISearchCategory search,
    IStorageCategory storage,
    IAdsCategory ads,
    INotificationsCategory notifications,
    ILeadsCategory leads,
    IAppWidgetsCategory appWidgets,
    IOrdersCategory orders,
    ISecureCategory secure,
    IStoriesCategory stories,
    ILeadFormsCategory leadForms,
    IPrettyCardsCategory prettyCards,
    IPodcastsCategory podcasts,
    IDonutCategory donut,
    IDownloadedGamesCategory downloadedGames,
    IServiceProvider provider,
    IAsrCategory asr,
    IShortVideoCategory shortVideo)
    : IVkApiCategories
{
    public IUsersCategory Users { get; } = users;
    public IFriendsCategory Friends { get; } = friends;
    public IStatusCategory Status { get; } = status;
    public IMessagesCategory Messages => provider.GetRequiredService<IMessagesCategory>(); // circular dependency issue
    public IGroupsCategory Groups => provider.GetRequiredService<IGroupsCategory>();
    public IExecuteCategory Execute => provider.GetRequiredService<IExecuteCategory>();
    public IAudioCategory Audio { get; } = audio;
    public IDatabaseCategory Database { get; } = database;
    public IUtilsCategory Utils { get; } = utils;
    public IWallCategory Wall { get; } = wall;
    public IBoardCategory Board { get; } = board;
    public IFaveCategory Fave { get; } = fave;
    public IVideoCategory Video { get; } = video;
    public IAccountCategory Account { get; } = account;
    public IPhotoCategory Photo { get; } = photo;
    public IDocsCategory Docs { get; } = docs;
    public ILikesCategory Likes { get; } = likes;
    public IPagesCategory Pages { get; } = pages;
    public IAppsCategory Apps { get; } = apps;
    public INewsFeedCategory NewsFeed { get; } = newsFeed;
    public IStatsCategory Stats { get; } = stats;
    public IGiftsCategory Gifts { get; } = gifts;
    public IMarketsCategory Markets { get; } = markets;
    public IAuthCategory Auth { get; } = auth;
    public IPollsCategory PollsCategory { get; } = pollsCategory;
    public ISearchCategory Search { get; } = search;
    public IStorageCategory Storage { get; } = storage;
    public IAdsCategory Ads { get; } = ads;
    public INotificationsCategory Notifications { get; } = notifications;
    public IWidgetsCategory Widgets { get; } = null!; // maybe later
    public ILeadsCategory Leads { get; } = leads;
    public IStreamingCategory Streaming { get; } = null!; // maybe later
    public IPlacesCategory Places { get; } = null!; // maybe later
    public INotesCategory Notes { get; set; } = null!; // maybe later
    public IAppWidgetsCategory AppWidgets { get; set; } = appWidgets;
    public IOrdersCategory Orders { get; set; } = orders;
    public ISecureCategory Secure { get; set; } = secure;
    public IStoriesCategory Stories { get; set; } = stories;
    public ILeadFormsCategory LeadForms { get; set; } = leadForms;
    public IPrettyCardsCategory PrettyCards { get; set; } = prettyCards;
    public IPodcastsCategory Podcasts { get; set; } = podcasts;
    public IDonutCategory Donut { get; } = donut;
    public IDownloadedGamesCategory DownloadedGames { get; } = downloadedGames;
    public IAsrCategory Asr { get; } = asr;
    public IShortVideoCategory ShortVideo { get; } = shortVideo;
}