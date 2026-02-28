namespace SecureLink.Core.Contracts;

public record FileResponse
{
    public required Guid Id { get; init; }

    // This is the filename provided by user
    // It is mapped to UserFilename in the DTO
    public required string Filename { get; init; }
    public required string ContentType { get; init; }
    public FileStatus Status { get; set; } = FileStatus.Pending;
    public string Metadata { get; init; } = "{}";
    public DateTimeOffset? CreatedAt { get; init; }
    public DateTimeOffset? LastModifiedAt { get; init; }
}
