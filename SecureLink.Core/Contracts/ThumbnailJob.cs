namespace SecureLink.Core.Contracts;

public record ThumbnailJob
{
    public required Guid FileId { get; init; }
    public required string Filename { get; init; }
    public required string StorageKey { get; init; }
    public int RetryCount { get; init; } = 0;
}
