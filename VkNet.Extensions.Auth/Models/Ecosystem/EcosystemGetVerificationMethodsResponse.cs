using System.Collections.ObjectModel;
using VkNet.Extensions.Auth.Models.Auth;

namespace VkNet.Extensions.Auth.Models.Ecosystem;

public record EcosystemGetVerificationMethodsResponse(ReadOnlyCollection<EcosystemVerificationMethod> Methods);

public record EcosystemVerificationMethod(LoginWay Name, int Priority, int Timeout, string Info, bool CanFallback);