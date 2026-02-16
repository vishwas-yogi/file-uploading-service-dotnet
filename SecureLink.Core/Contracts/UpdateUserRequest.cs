namespace SecureLink.Core.Contracts;

public record UpdateUserRequest
{
    public required Guid Id { get; init; }
    public string? Username { get; init; }
    public string? Name { get; init; }
    public string? Email { get; init; }
};
