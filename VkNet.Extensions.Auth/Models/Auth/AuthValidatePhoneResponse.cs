namespace VkNet.Extensions.Auth.Models.Auth;

public record AuthValidatePhoneResponse(LoginWay ValidationType, LoginWay ValidationResend, string Sid, int Delay, int CodeLength, bool LibverifySupport, string Phone);