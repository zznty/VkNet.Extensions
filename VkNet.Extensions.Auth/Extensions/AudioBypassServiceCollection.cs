using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VkNet.Abstractions;
using VkNet.Abstractions.Authorization;
using VkNet.Extensions.Auth.Abstractions.Categories;
using VkNet.Extensions.Auth.Abstractions.Interop;
using VkNet.Extensions.Auth.Categories;
using VkNet.Extensions.Auth.Flows;
using VkNet.Extensions.Auth.Interop.Win32;
using VkNet.Extensions.Auth.Models.Auth;
using VkNet.Extensions.Auth.Utils;
using VkNet.Extensions.DependencyInjection;
using VkNet.Extensions.DependencyInjection.Abstractions;
using IAuthCategory = VkNet.Extensions.Auth.Abstractions.Categories.IAuthCategory;
using VkApiInvoke = VkNet.Extensions.Auth.Utils.VkApiInvoke;

namespace VkNet.Extensions.Auth.Extensions;

public static class AudioBypassServiceCollection
{
	public static T AddVkNetWithAuth<T>(this T collection, VkAuthRegisterOptions? options = null) where T : IServiceCollection
	{
        ArgumentNullException.ThrowIfNull(collection);
        
		collection.TryAddSingleton<LibVerifyClient>();
		collection.TryAddTransient<ITokenRefreshHandler, TokenRefreshHandler>();
			
		collection.TryAddKeyedTransient<IAuthorizationFlow>(AndroidGrantType.Password, (s, _) => s.GetRequiredService<PasswordAuthorizationFlow>());
		collection.TryAddKeyedTransient(AndroidGrantType.PhoneConfirmationSid,
			(s, _) => s.GetRequiredKeyedService<IAuthorizationFlow>(AndroidGrantType.Password));
		collection.TryAddKeyedTransient<IAuthorizationFlow>(AndroidGrantType.WithoutPassword, (s, _) => s.GetRequiredService<WithoutPasswordAuthorizationFlow>());
		collection.TryAddKeyedTransient<IAuthorizationFlow>(AndroidGrantType.Passkey, (s, _) => s.GetRequiredService<PasskeyAuthorizationFlow>());
			
		collection.TryAddTransient<IAuthorizationFlow, VkAndroidAuthorizationFlow>();
		
		collection.TryAddTransient<IEcosystemCategory, EcosystemCategory>();
		
		if (WindowsPasskeyApi.IsSupported)
			collection.TryAddSingleton<IPlatformPasskeyApi, WindowsPasskeyApi>();

		collection.AddVkNet();

		collection.RemoveAll<IVkApiInvoke>();
		
		collection.AddHttpClient<IVkApiInvoke, VkApiInvoke>((options ?? VkAuthRegisterOptions.DefaultAndroid).ConfigureClient)
			.AddTypedClient<PasswordAuthorizationFlow>()
			.AddTypedClient<WithoutPasswordAuthorizationFlow>()
			.AddTypedClient<PasskeyAuthorizationFlow>()
			.AddTypedClient<IAuthCategory, AuthCategory>()
			.AddTypedClient<ILoginCategory, LoginCategory>();

		return collection;
	}
}