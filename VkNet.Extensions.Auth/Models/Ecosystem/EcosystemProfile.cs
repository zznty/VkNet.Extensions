using Newtonsoft.Json;

namespace VkNet.Extensions.Auth.Models.Ecosystem;

public record EcosystemProfile(string FirstName, string LastName, string Phone, bool Has2Fa, bool CanUnbindPhone, [property: JsonProperty("photo_200")] string? Photo200);