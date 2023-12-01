using VkNet.Extensions.Auth.Models.Auth;
using VkNet.Extensions.Auth.Models.Ecosystem;

namespace VkNet.Extensions.Auth.Abstractions.Categories;

public interface IEcosystemCategory
{
    Task<EcosystemSendOtpResponse> SendOtpSmsAsync(string sid);
    Task<EcosystemSendOtpResponse> SendOtpPushAsync(string sid);
    Task<EcosystemSendOtpResponse> SendOtpCallResetAsync(string sid);
    Task<EcosystemCheckOtpResponse> CheckOtpAsync(string sid, LoginWay verificationMethod, string code);
    Task<EcosystemGetVerificationMethodsResponse> GetVerificationMethodsAsync(string sid);
}