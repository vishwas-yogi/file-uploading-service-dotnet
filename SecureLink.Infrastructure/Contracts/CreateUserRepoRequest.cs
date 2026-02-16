namespace SecureLink.Infrastructure.Contracts;

public record CreateUserRepoRequest
{
    public required Guid Id { get; init; }
    public required string Username { get; init; }
    public required string Name { get; init; }
    public string? Email { get; init; }
    public required string PasswordHash { get; init; }
}
