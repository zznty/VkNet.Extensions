namespace VkNet.Extensions.Auth.Models.Ecosystem;

public record EcosystemCheckOtpResponse(string Sid, bool ProfileExist, bool CanSkipPassword, EcosystemProfile Profile);