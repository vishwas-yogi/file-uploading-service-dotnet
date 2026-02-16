namespace SecureLink.Core.Contracts;

public record UserResponse
{
    public required Guid Id { get; init; }
    public required string Username { get; init; }
    public required string? Email { get; init; }
    public required string Name { get; init; }
    public required DateTimeOffset? CreatedAt { get; init; }
    public required DateTimeOffset? LastModifiedAt { get; init; }
}
