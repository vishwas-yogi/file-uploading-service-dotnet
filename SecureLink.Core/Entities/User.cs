namespace SecureLink.Core.Entities;

public class User
{
    public required Guid Id { get; set; }
    public required string Username { get; set; }
    public string? Email { get; set; }
    public required string Name { get; set; }
    public required string PasswordHash { get; set; }
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? LastModifiedAt { get; set; }
    // TODO: add a field for soft delete
}
