namespace SecureLink.Core.Contracts;

public record RegisterRequest
{
    public required string Username { get; init; }
    public required string Password { get; init; }
    public required string Name { get; init; }
    public string? Email { get; init; }
}
