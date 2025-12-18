using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VkNet.Abstractions;
using VkNet.Abstractions.Category;
using VkNet.Abstractions.Core;
using VkNet.Abstractions.Utils;
using VkNet.Categories;
using VkNet.Extensions.DependencyInjection.Abstractions;
using VkNet.Extensions.DependencyInjection.Services;
using VkNet.Utils;
using ICaptchaHandler = VkNet.Extensions.DependencyInjection.Abstractions.ICaptchaHandler;

namespace VkNet.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    extension<T>(T collection) where T : IServiceCollection
    {
        /// <summary>
        /// Регистрирует необходимые сервисы и клиент VK API
        /// </summary>
        /// <remarks>
        /// Переопределения сервисов или хранилищ должны быть зарегистрованы в коллекцию до вызова этого метода
        /// </remarks>
        /// <seealso cref="IVkApi"/>
        /// <seealso cref="IVkTokenStore"/>
        /// <seealso cref="IAsyncCaptchaSolver"/>
        public T AddVkNet()
        {
            return AddVkNet(collection, (s, c) => { });
        }

        /// <summary>
        /// Регистрирует необходимые сервисы и клиент Vk API
        /// </summary>
        /// <remarks>
        /// Переопределения сервисов или хранилищ должны быть зарегистрованы в коллекцию до вызова этого метода
        /// </remarks>
        /// <param name="configureHttpClient">Обработчик дополнительной настройки <see cref="HttpClient"/> клиента VK API</param>
        /// <seealso cref="IVkApi"/>
        /// <seealso cref="IVkTokenStore"/>
        /// <seealso cref="IAsyncCaptchaSolver"/>
        public T AddVkNet(Action<HttpClient> configureHttpClient)
        {
            return AddVkNet(collection, (s, c) => configureHttpClient(c));
        }

        /// <summary>
        /// Регистрирует необходимые сервисы и клиент Vk API
        /// </summary>
        /// <remarks>
        /// Переопределения сервисов или хранилищ должны быть зарегистрованы в коллекцию до вызова этого метода
        /// </remarks>
        /// <param name="configureHttpClient">Обработчик дополнительной настройки <see cref="HttpClient"/> клиента VK API</param>
        /// <seealso cref="IVkApi"/>
        /// <seealso cref="IVkTokenStore"/>
        /// <seealso cref="IAsyncCaptchaSolver"/>
        public T AddVkNet(Action<IServiceProvider, HttpClient> configureHttpClient)
        {
            ArgumentNullException.ThrowIfNull(collection);

            collection.AddHttpClient<IVkApiInvoke, VkApiInvoke>((provider, client) =>
            {
                client.BaseAddress = new("https://api.vk.com/method/");
                configureHttpClient(provider, client);
            });
        
            collection.TryAddSingleton<ICaptchaHandler, AsyncCaptchaHandler>();
            collection.TryAddTransient<IVkApiAuthAsync, VkApiAuth>();
            collection.TryAddSingleton<IVkTokenStore, DefaultVkTokenStore>();
            collection.TryAddTransient<IVkApi, Services.VkApi>();
            collection.TryAddSingleton<IAsyncCaptchaSolver>(_ => null!);

            collection.TryAddTransient<IVkApiAuth>(s => s.GetRequiredService<IVkApiAuthAsync>());
            collection.TryAddTransient<IVkInvoke>(s => s.GetRequiredService<IVkApi>());
            collection.TryAddSingleton<IAsyncRateLimiter>(_ => new AsyncRateLimiter(TimeSpan.FromSeconds(1), 3));
            collection.RegisterDefaultDependencies();

            // unregister unsupported types
            collection.RemoveAll<IRestClient>();
            collection.RemoveAll<IWebProxy>();
            collection.RemoveAll<IAwaitableConstraint>();
            collection.RemoveAll<INeedValidationHandler>();
        
            AddCategories(collection);
        
            return collection;
        }
    }

    private static void AddCategories(IServiceCollection collection)
    {
        collection.TryAddTransient<IVkApiCategories, VkApiCategories>();
        
        collection.TryAddTransient<IUsersCategory, UsersCategory>();
        collection.TryAddTransient<IFriendsCategory, FriendsCategory>();
        collection.TryAddTransient<IStatsCategory, StatsCategory>();
        collection.TryAddTransient<IMessagesCategory, MessagesCategory>();
        collection.TryAddTransient<IGroupsCategory, GroupsCategory>();
        collection.TryAddTransient<IAudioCategory, AudioCategory>();
        collection.TryAddTransient<IDatabaseCategory, DatabaseCategory>();
        collection.TryAddTransient<IUtilsCategory, UtilsCategory>();
        collection.TryAddTransient<IWallCategory, WallCategory>();
        // collection.TryAddTransient<IBoardCategory, BoardCategory>();
        collection.TryAddTransient<IFaveCategory, FaveCategory>();
        collection.TryAddTransient<IVideoCategory, VideoCategory>();
        collection.TryAddTransient<IAccountCategory, AccountCategory>();
        collection.TryAddTransient<IPhotoCategory, PhotoCategory>();
        collection.TryAddTransient<IDocsCategory, DocsCategory>();
        collection.TryAddTransient<ILikesCategory, LikesCategory>();
        collection.TryAddTransient<IPagesCategory, PagesCategory>();
        collection.TryAddTransient<IAppsCategory, AppsCategory>();
        collection.TryAddTransient<INewsFeedCategory, NewsFeedCategory>();
        collection.TryAddTransient<IStatsCategory, StatsCategory>();
        collection.TryAddTransient<IGiftsCategory, GiftsCategory>();
        collection.TryAddTransient<IMarketsCategory, MarketsCategory>();
        collection.TryAddTransient<IAuthCategory, AuthCategory>();
        collection.TryAddTransient<IExecuteCategory, ExecuteCategory>();
        collection.TryAddTransient<IPollsCategory, PollsCategory>();
        collection.TryAddTransient<ISearchCategory, SearchCategory>();
        collection.TryAddTransient<IStorageCategory, StorageCategory>();
        collection.TryAddTransient<IAdsCategory, AdsCategory>();
        collection.TryAddTransient<INotificationsCategory, NotificationsCategory>();
        collection.TryAddTransient<IWidgetsCategory, WidgetsCategory>();
        collection.TryAddTransient<ILeadsCategory, LeadsCategory>();
        collection.TryAddTransient<IStreamingCategory, StreamingCategory>();
        collection.TryAddTransient<IPlacesCategory, PlacesCategory>();
        collection.TryAddTransient<INotesCategory, NotesCategory>();
        collection.TryAddTransient<IAppWidgetsCategory, AppWidgetsCategory>();
        collection.TryAddTransient<IOrdersCategory, OrdersCategory>();
        collection.TryAddTransient<ISecureCategory, SecureCategory>();
        collection.TryAddTransient<IStoriesCategory, StoriesCategory>();
        collection.TryAddTransient<ILeadFormsCategory, LeadFormsCategory>();
        collection.TryAddTransient<IPrettyCardsCategory, PrettyCardsCategory>();
        collection.TryAddTransient<IPodcastsCategory, PodcastsCategory>();
        collection.TryAddTransient<IDonutCategory, DonutCategory>();
        collection.TryAddTransient<IDownloadedGamesCategory, DownloadedGamesCategory>();
        collection.TryAddTransient<IStatusCategory, StatusCategory>();
        collection.TryAddTransient<IAsrCategory, AsrCategory>();
        collection.TryAddTransient<IShortVideoCategory, ShortVideoCategory>();
    }
}