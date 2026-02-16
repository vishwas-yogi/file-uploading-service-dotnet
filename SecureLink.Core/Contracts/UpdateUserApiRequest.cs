namespace SecureLink.Core.Contracts;

public record UpdateUserApiRequest
{
    public string? Username { get; init; }
    public string? Name { get; init; }
    public string? Email { get; init; }
}
