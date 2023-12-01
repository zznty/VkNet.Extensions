using System.Collections.ObjectModel;

namespace VkNet.Extensions.Auth.Models.Auth;

public record AuthRefreshTokensResponse(ReadOnlyCollection<RefreshedSlot> Success);

public record RefreshedSlot(int Index, long UserId, TokenInfo AccessToken);

public record TokenInfo(string Token, long ExpiresIn);
