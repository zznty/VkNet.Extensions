﻿using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using VkNet.Abstractions;
using VkNet.Abstractions.Core;
using VkNet.Exception;
using VkNet.Extensions.Auth.Abstractions;
using VkNet.Extensions.DependencyInjection.Abstractions;
using VkNet.Model;
using VkNet.Utils;
using VkNet.Utils.JsonConverter;

namespace VkNet.Extensions.Auth.Utils;

public class VkApiInvoke(
    HttpClient client,
    ICaptchaHandler handler,
    IVkApiVersionManager versionManager,
    IVkTokenStore tokenStore,
    ILanguageService languageService,
    IAsyncRateLimiter rateLimiter,
    ITokenRefreshHandler tokenRefreshHandler,
    IDeviceIdStore deviceIdStore)
    : IVkApiInvoke
{
    private readonly IEnumerable<JsonConverter> _defaultConverters = new JsonConverter[]
    {
        new VkCollectionJsonConverter(),
        new UnixDateTimeConverter(),
        new AttachmentJsonConverter(),
        new StringEnumConverter(),
    };

    private async ValueTask TryAddRequiredParameters(IDictionary<string, string> parameters, bool skipAuthorization)
    {
        parameters.TryAdd("v", versionManager.Version);
        parameters.TryAdd("lang", languageService.GetLanguage()?.ToString() ?? "ru");
        
        if (await deviceIdStore.GetDeviceIdAsync() is { } deviceId)
            parameters.TryAdd("device_id", deviceId);
        
        if (!skipAuthorization)
            parameters.TryAdd("access_token", tokenStore.Token);
    }

    private JsonSerializerSettings CreateSettings(IEnumerable<JsonConverter> userConverters)
    {
        var converters = _defaultConverters.ToList(); // i actually wanna clone the array here
        converters.AddRange(userConverters);
        
        return new()
        {
            Converters = converters,
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new SnakeCaseNamingStrategy()
            },
            MaxDepth = null,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };
    }

    public VkResponse Call(string methodName, VkParameters parameters, bool skipAuthorization = false,
        params JsonConverter[] jsonConverters)
    {
        throw new NotImplementedException("Interface implementation is not provided");
    }

    public T? Call<T>(string methodName, VkParameters parameters, bool skipAuthorization = false,
                      params JsonConverter[] jsonConverters)
    {
        return CallAsync<T>(methodName, parameters, skipAuthorization, jsonConverters).GetAwaiter().GetResult();
    }

    private async Task<T?> CallAsync<T>(string methodName, VkParameters parameters, bool skipAuthorization, JsonConverter[] jsonConverters, CancellationToken token = default)
    {
        var json = await InvokeInternalAsync(methodName, parameters, skipAuthorization, token);

        return json.ToObject<T>(JsonSerializer.Create(CreateSettings(jsonConverters)));
    }

    public async Task<VkResponse> CallAsync(string methodName, VkParameters parameters, bool skipAuthorization = false, CancellationToken token = default)
    {
        var json = await InvokeInternalAsync(methodName, parameters, skipAuthorization, token);

        return new(json)
        {
            RawJson = json.ToString()
        };
    }

    public Task<T?> CallAsync<T>(string methodName, VkParameters parameters, bool skipAuthorization = false, CancellationToken token = default)
    {
        return CallAsync<T>(methodName, parameters, skipAuthorization, Array.Empty<JsonConverter>(), token);
    }

    public string Invoke(string methodName, IDictionary<string, string> parameters, bool skipAuthorization = false)
    {
        return InvokeAsync(methodName, parameters, skipAuthorization).GetAwaiter().GetResult();
    }

    public async Task<string> InvokeAsync(string methodName, IDictionary<string, string> parameters, bool skipAuthorization = false, CancellationToken token = default)
    {
        var json = await InvokeInternalAsync(methodName, parameters, skipAuthorization, token);
        return json.ToString();
    }

    private Task<JToken> InvokeInternalAsync(string methodName, IDictionary<string, string> parameters, bool skipAuthorization, CancellationToken token = default)
    {
        return handler.Perform(async (sid, key) =>
        {
            await TryAddRequiredParameters(parameters, skipAuthorization);
            
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
                await tokenRefreshHandler.RefreshTokenAsync(tokenStore.Token) is not { } newToken)
                throw new VkApiException(vkError);
                
            parameters["access_token"] = newToken;
            return await InvokeInternalAsync(methodName, parameters, skipAuthorization);
        });
    }

    public DateTimeOffset? LastInvokeTime { get; private set;}
    public TimeSpan? LastInvokeTimeSpan => DateTimeOffset.Now - LastInvokeTime;
}