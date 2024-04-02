using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using VkNet.Abstractions;
using VkNet.Abstractions.Core;
using VkNet.Exception;
using VkNet.Extensions.DependencyInjection.Abstractions;
using VkNet.Model;
using VkNet.Utils;
using VkNet.Utils.JsonConverter;

namespace VkNet.Extensions.DependencyInjection.Services;

public class VkApiInvoke(
    HttpClient client,
    ICaptchaHandler handler,
    IVkApiVersionManager versionManager,
    IVkTokenStore tokenStore,
    ILanguageService languageService,
    IAsyncRateLimiter rateLimiter,
    ITokenRefreshHandler? tokenRefreshHandler = null)
    : IVkApiInvoke
{
    private readonly JsonSerializer _defaultSerializer = JsonSerializer.Create(new()
    {
        Converters = [
            new VkCollectionJsonConverter(),
            new UnixDateTimeConverter(),
            new AttachmentJsonConverter(),
            new StringEnumConverter()
        ],
        ContractResolver = new DefaultContractResolver
        {
            NamingStrategy = new SnakeCaseNamingStrategy()
        },
        MaxDepth = null,
        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
    });

    protected virtual ValueTask TryAddRequiredParametersAsync(IDictionary<string, string> parameters, bool skipAuthorization)
    {
        parameters.TryAdd("v", versionManager.Version);
        parameters.TryAdd("lang", languageService.GetLanguage()?.ToString() ?? "ru");
        if (!skipAuthorization)
            parameters.TryAdd("access_token", tokenStore.Token);
        
        return default;
    }

    public VkResponse Call(string methodName, VkParameters parameters, bool skipAuthorization = false,
        params JsonConverter[] jsonConverters)
    {
        throw new NotSupportedException("Synchronous method calls are not supported by this implementation. Use async overload instead.");
    }

    public T? Call<T>(string methodName, VkParameters parameters, bool skipAuthorization = false,
                      params JsonConverter[] jsonConverters)
    {
        if (jsonConverters.Length > 0)
            throw new ArgumentException("This implementation does not support JsonConverters", nameof(jsonConverters));
        
        return CallAsync<T>(methodName, parameters, skipAuthorization).GetAwaiter().GetResult();
    }

    public async Task<VkResponse> CallAsync(string methodName, VkParameters parameters, bool skipAuthorization = false, CancellationToken token = default)
    {
        var json = await InvokeInternalAsync(methodName, parameters, skipAuthorization, token);

        return new(json);
    }

    public async Task<T?> CallAsync<T>(string methodName, VkParameters parameters, bool skipAuthorization = false, CancellationToken token = default)
    {
        var json = await InvokeInternalAsync(methodName, parameters, skipAuthorization, token);

        return json.ToObject<T>(_defaultSerializer);
    }

    public string Invoke(string methodName, IDictionary<string, string> parameters, bool skipAuthorization = false)
    {
        throw new NotSupportedException("Synchronous method calls are not supported by this implementation. Use async overload instead.");
    }

    public async Task<string> InvokeAsync(string methodName, IDictionary<string, string> parameters, bool skipAuthorization = false, CancellationToken token = default)
    {
        var json = await InvokeInternalAsync(methodName, parameters, skipAuthorization, token);
        return json.ToString();
    }

    private async Task<JToken> InvokeInternalAsync(string methodName, IDictionary<string, string> parameters, bool skipAuthorization, CancellationToken token = default)
    {
        await TryAddRequiredParametersAsync(parameters, skipAuthorization);
        
        return await handler.Perform(async (sid, key) =>
        {
            if (sid is { } captchaSid)
            {
                parameters.Add("captcha_sid", captchaSid.ToString());
                parameters.Add("captcha_key", key);
            }

            await rateLimiter.WaitNextAsync(token);

            var content = new FormUrlEncodedContent(parameters);

            using var response = await client.SendAsync(new()
            {
                Method = HttpMethod.Post,
                RequestUri = new(methodName, UriKind.Relative),
                Content = content,
            }, HttpCompletionOption.ResponseHeadersRead, token);
            
            LastInvokeTime = DateTimeOffset.Now;

            response.EnsureSuccessStatusCode();

            await using var stream = await response.Content.ReadAsStreamAsync(token);
            using var textReader = new StreamReader(stream, Encoding.UTF8, leaveOpen: true);
            await using var reader = new JsonTextReader(textReader) { CloseInput = false };

            var obj = await JToken.ReadFromAsync(reader, token);

            if (obj["error"] is not { } error) return obj["response"]!;
            
            var vkError = error.ToObject<VkError>();

            if (vkError?.ErrorCode is not (5 or 1117 or 1114) || // token has expired
                tokenRefreshHandler == null ||
                await tokenRefreshHandler.RefreshTokenAsync(tokenStore.Token) is not { } newToken)
                throw new VkApiException(vkError);
                
            parameters["access_token"] = newToken;
            return await InvokeInternalAsync(methodName, parameters, skipAuthorization);
        });
    }

    public DateTimeOffset? LastInvokeTime { get; private set;}
    public TimeSpan? LastInvokeTimeSpan => DateTimeOffset.Now - LastInvokeTime;
}