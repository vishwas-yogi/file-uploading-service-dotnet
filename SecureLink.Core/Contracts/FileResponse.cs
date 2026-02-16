namespace SecureLink.Core.Contracts;

public record FileResponse
{
    public required Guid Id { get; init; }
    public required string UserFilename { get; init; }
    public required string ContentType { get; init; }
    public required string Location { get; init; }
    public required Guid Owner { get; init; }
    public string Metadata { get; init; } = "{}";
    public DateTimeOffset? CreatedAt { get; init; }
    public DateTimeOffset? LastModifiedAt { get; init; }
}
