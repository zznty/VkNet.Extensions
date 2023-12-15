using VkNet.Extensions.Auth.Models.Auth;
using VkNet.Extensions.Auth.Models.Ecosystem;

namespace VkNet.Extensions.Auth.Abstractions.Categories;

public interface IEcosystemCategory
{
    Task<EcosystemSendOtpResponse> SendOtpSmsAsync(string sid, CancellationToken token = default);
    Task<EcosystemSendOtpResponse> SendOtpPushAsync(string sid, CancellationToken token = default);
    Task<EcosystemSendOtpResponse> SendOtpCallResetAsync(string sid, CancellationToken token = default);
    Task<EcosystemCheckOtpResponse> CheckOtpAsync(string sid, LoginWay verificationMethod, string code, CancellationToken token = default);
    Task<EcosystemGetVerificationMethodsResponse> GetVerificationMethodsAsync(string sid, CancellationToken token = default);
}