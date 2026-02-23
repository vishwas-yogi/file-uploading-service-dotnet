using System.Text.Json.Serialization;

namespace SecureLink.Core.Contracts;

public record LoginApiRequest
{
    public required string Username { get; init; }

    [JsonIgnore]
    public required string Password { get; init; }
}
