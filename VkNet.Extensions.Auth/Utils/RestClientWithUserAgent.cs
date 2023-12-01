using System.Net;
using Microsoft.Extensions.Logging;
using VkNet.Abstractions.Utils;
using VkNet.Utils;

namespace VkNet.Extensions.Auth.Utils;

/// <inheritdoc cref="IRestClient" />
public class RestClientWithUserAgent : RestClient
{
	private static readonly IDictionary<string, string> VkHeaders = new Dictionary<string, string>
	{
		{ "User-Agent", "VKAndroidApp/8.50-17564 (Android 12; SDK 32; arm64-v8a; Pixel 4a; ru; 2960x1440)" },
		{ "X-VK-Android-Client", "new" },
		// { "X-Quic", "1" }
	};

	public RestClientWithUserAgent(HttpClient httpClient, ILogger<RestClient> logger) : base(httpClient, logger)
	{
		foreach (var header in VkHeaders)
		{
			httpClient.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value);
		}

		httpClient.DefaultRequestVersion = HttpVersion.Version20;
		httpClient.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrHigher;
	}
}