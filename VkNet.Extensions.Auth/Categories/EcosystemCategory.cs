using VkNet.Abstractions;
using VkNet.Extensions.Auth.Abstractions.Categories;
using VkNet.Extensions.Auth.Models.Auth;
using VkNet.Extensions.Auth.Models.Ecosystem;

namespace VkNet.Extensions.Auth.Categories;

public class EcosystemCategory(IVkApiInvoke invoke) : IEcosystemCategory
{
    public Task<EcosystemSendOtpResponse> SendOtpSmsAsync(string sid, CancellationToken token = default)
    {
        return invoke.CallAsync<EcosystemSendOtpResponse>("ecosystem.sendOtpSms", new()
        {
            { "sid", sid },
            { "api_id", 2274003 }
        }, token: token);
    }
    
    public Task<EcosystemSendOtpResponse> SendOtpPushAsync(string sid, CancellationToken token = default)
    {
        return invoke.CallAsync<EcosystemSendOtpResponse>("ecosystem.sendOtpPush", new()
        {
            { "sid", sid },
            { "api_id", 2274003 }
        }, token: token);
    }
    
    public Task<EcosystemSendOtpResponse> SendOtpCallResetAsync(string sid, CancellationToken token = default)
    {
        return invoke.CallAsync<EcosystemSendOtpResponse>("ecosystem.sendOtpCallReset", new()
        {
            { "sid", sid },
            { "api_id", 2274003 }
        }, token: token);
    }

    public Task<EcosystemCheckOtpResponse> CheckOtpAsync(string sid, LoginWay verificationMethod, string code, CancellationToken token = default)
    {
        return invoke.CallAsync<EcosystemCheckOtpResponse>("ecosystem.checkOtp", new()
        {
            { "sid", sid },
            { "api_id", 2274003 },
            { "verification_method", verificationMethod },
            { "code", code }
        }, token: token);
    }

    public Task<EcosystemGetVerificationMethodsResponse> GetVerificationMethodsAsync(string sid, CancellationToken token = default)
    {
        return invoke.CallAsync<EcosystemGetVerificationMethodsResponse>("ecosystem.getVerificationMethods", new()
        {
            { "sid", sid },
            { "api_id", 2274003 }
        }, token: token);
    }
}