namespace SecureLink.Core.Contracts;

public record CreateUserRequest
{
    public required string Name { get; init; }
    public required string Username { get; init; }
    public string? Email { get; init; }
    public required string PasswordHash { get; init; }
};
