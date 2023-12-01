namespace VkNet.Extensions.Auth.Models.LibVerify;

public record AttemptResponse(VerifyResponseStatus Status, string Token);