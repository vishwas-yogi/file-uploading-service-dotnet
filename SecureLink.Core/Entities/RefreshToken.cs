namespace SecureLink.Core.Entities;

public class RefreshToken
{
    public required Guid Id { get; set; }
    public required Guid UserId { get; set; }
    public required string Value { get; set; }
    public required DateTimeOffset ExpiresAt { get; set; }
    public required DateTimeOffset? CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
