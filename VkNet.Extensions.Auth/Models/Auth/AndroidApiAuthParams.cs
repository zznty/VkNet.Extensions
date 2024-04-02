using Newtonsoft.Json;
using VkNet.Enums.Filters;
using VkNet.Enums.SafetyEnums;
using VkNet.Enums.StringEnums;
using VkNet.Extensions.Auth.Models.Ecosystem;
using VkNet.Model;
using VkNet.Utils.JsonConverter;

namespace VkNet.Extensions.Auth.Models.Auth;

public delegate ValueTask<string?> CodeRequestedDelegate(LoginWay requestedLoginWay, AuthState state);
public delegate ValueTask<LoginWay> VerificationMethodRequestedDelegate(IReadOnlyCollection<EcosystemVerificationMethod> verificationMethods, AuthState state);

/// <inheritdoc />
public record AndroidApiAuthParams(string? Login, string? Sid, CodeRequestedDelegate? CodeRequestedAsync, IEnumerable<LoginWay>? SupportedWays = null,
    string? Password = null, string? PasskeyData = null, VerificationMethodRequestedDelegate? VerificationMethodRequestedAsync = null, CancellationToken CancellationToken = default) : IApiAuthParams
{
    public AndroidApiAuthParams() : this(null, null, null)
    {
        IsAnonymous = true;
    }

    /// <inheritdoc />
    public ulong ApplicationId { get; set; } = 2274003;

    /// <inheritdoc />
    public string? Login { get; set; } = Login;

    /// <inheritdoc />
    public string? Password { get; set; } = Password;

    [Obsolete("Not implemented", true)]
    public Settings? Settings { get; set; }

    [Obsolete($"Set {nameof(CodeRequestedAsync)} event", true)]
    public Func<string>? TwoFactorAuthorization { get; set; }
    
    [Obsolete($"Set {nameof(CodeRequestedAsync)} event", true)]
    public Task<string>? TwoFactorAuthorizationAsync { get; set; }

    /// <inheritdoc />
    public string? AccessToken { get; set; }
    /// <inheritdoc />
    public int TokenExpireTime { get; set; }
    /// <inheritdoc />
    public long UserId { get; set; }
    [Obsolete("Not implemented", true)]
    public ulong? CaptchaSid { get; set; }
    [Obsolete("Not implemented", true)]
    public string? CaptchaKey { get; set; }
    [Obsolete("Not implemented", true)]
    public string? Host { get; set; }
    [Obsolete("Not implemented", true)]
    public int? Port { get; set; }
    [Obsolete("Not implemented", true)]
    public string? ProxyLogin { get; set; }
    [Obsolete("Not implemented", true)]
    public string? ProxyPassword { get; set; }

    [Obsolete($"Use {nameof(Login)} instead", true)]
    public string? Phone { get; set; } = Login;

    /// <inheritdoc />
    public string ClientSecret { get; set; } = "hHbZxrka2uZ6jB1inYsH";
    /// <inheritdoc />
    public bool? ForceSms { get; set; }
    [Obsolete("Not implemented", true)]
    public Display? Display { get; set; }
    [Obsolete("Not implemented", true)]
    public Uri? RedirectUri { get; set; }
    /// <inheritdoc />
    public string? State { get; set; }
    [Obsolete("Not implemented", true)]
    public bool? TwoFactorSupported { get; set; } = true;

    [Obsolete($"Use {nameof(AndroidGrantType)} instead", true)]
    public GrantType? GrantType { get; set; }

    [Obsolete("Not implemented", true)]
    public ResponseType? ResponseType { get; set; }
    [Obsolete("Not implemented", true)]
    public bool? Revoke { get; set; }
    public string? Code { get; set; }
    [Obsolete("Not implemented", true)]
    public bool IsTokenUpdateAutomatically { get; set; }
    
    public bool IsAnonymous { get; }

    public bool IsValid =>IsAnonymous ||
                          (!string.IsNullOrEmpty(Login) && !string.IsNullOrEmpty(Sid) &&
                           CodeRequestedAsync is not null && SupportedWays?.Any() is true);

    public IEnumerable<LoginWay> SupportedWays { get; init; } = SupportedWays ?? new[]
    {
        LoginWay.Password, LoginWay.Push, LoginWay.Sms, LoginWay.CallReset, LoginWay.ReserveCode,
        LoginWay.Codegen, LoginWay.Email, LoginWay.Passkey
    };
}

[JsonConverter(typeof(SafetyEnumJsonConverter))]
public class AndroidGrantType : SafetyEnum<AndroidGrantType>
{
    public static readonly AndroidGrantType Password = RegisterPossibleValue("password");
    public static readonly AndroidGrantType Passkey = RegisterPossibleValue("passkey");
    public static readonly AndroidGrantType WithoutPassword = RegisterPossibleValue("without_password");
    public static readonly AndroidGrantType PhoneConfirmationSid = RegisterPossibleValue("phone_confirmation_sid");
}

public record AuthState(string Sid);

public record ProfileAuthState(string Sid, EcosystemProfile Profile) : AuthState(Sid);

public record TwoFactorAuthState(string Sid, string? PhoneMask, bool IsSms, int? CodeLength) : AuthState(Sid);

public record VerificationAuthState(string Sid, string Info, int CodeLength) : AuthState(Sid);