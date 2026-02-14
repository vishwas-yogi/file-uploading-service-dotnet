namespace SecureLink.Core.Contracts;

public record FileResponse
{
    public required Guid Id { get; init; }
    public required string UserFilename { get; init; }
    public required string ContentType { get; init; }
    public required string Location { get; init; }
    public required Guid Owner { get; init; }
    public string Metadata { get; init; } = "{}";
    public required DateTimeOffset? CreatedAt { get; init; } = DateTimeOffset.UtcNow;
    public required DateTimeOffset? LastModifiedAt { get; init; } = DateTimeOffset.UtcNow;
}
