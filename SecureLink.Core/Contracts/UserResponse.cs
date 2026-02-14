namespace SecureLink.Core.Contracts;

public record UserResponse
{
    public required Guid Id { get; init; }
    public required string Username { get; init; }
    public required string? Email { get; init; }
    public required string Name { get; init; }
    public DateTimeOffset? CreatedAt { get; init; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? LastModifiedAt { get; init; } = DateTimeOffset.UtcNow;
}
