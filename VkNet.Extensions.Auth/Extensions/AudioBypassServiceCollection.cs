using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VkNet.Abstractions;
using VkNet.Abstractions.Authorization;
using VkNet.Abstractions.Utils;
using VkNet.Extensions.Auth.Abstractions;
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
using VkApiAuth = VkNet.Extensions.Auth.Utils.VkApiAuth;
using VkApiInvoke = VkNet.Extensions.Auth.Utils.VkApiInvoke;

namespace VkNet.Extensions.Auth.Extensions;

public static class AudioBypassServiceCollection
{
	public static IServiceCollection AddVkNetAuth(this IServiceCollection services)
	{
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddSingleton<FakeSafetyNetClient>();
		services.TryAddSingleton<LibVerifyClient>();
		services.TryAddSingleton<IRestClient, RestClientWithUserAgent>();
		services.TryAddSingleton<IDeviceIdStore, DefaultDeviceIdStore>();
		services.TryAddSingleton<ITokenRefreshHandler, TokenRefreshHandler>();
		services.TryAddSingleton<IVkApiInvoke, VkApiInvoke>();
		services.TryAddSingleton<IVkApiAuthAsync, VkApiAuth>();
			
		services.TryAddKeyedSingleton<IAuthorizationFlow, PasswordAuthorizationFlow>(AndroidGrantType.Password);
		services.TryAddKeyedSingleton(AndroidGrantType.PhoneConfirmationSid,
			(s, _) => s.GetRequiredKeyedService<IAuthorizationFlow>(AndroidGrantType.Password));
		services.TryAddKeyedSingleton<IAuthorizationFlow, WithoutPasswordAuthorizationFlow>(AndroidGrantType.WithoutPassword);
		services.TryAddKeyedSingleton<IAuthorizationFlow, PasskeyAuthorizationFlow>(AndroidGrantType.Passkey);
			
		services.TryAddSingleton<IAuthorizationFlow, VkAndroidAuthorizationFlow>();
			
		services.TryAddSingleton<IAuthCategory, AuthCategory>();
		services.TryAddSingleton<ILoginCategory, LoginCategory>();
		services.TryAddSingleton<IEcosystemCategory, EcosystemCategory>();
		
		if (WindowsPasskeyApi.IsSupported)
			services.TryAddSingleton<IPlatformPasskeyApi, WindowsPasskeyApi>();

		return services;
	}
}