using SecureLink.Core.Contracts;

namespace SecureLink.Infrastructure.Contracts;

public record FilePersistRepoRequest
{
    public required string Filename { get; init; }
    public required string UserFilename { get; init; }
    public required string ContentType { get; init; }
    public required Guid Owner { get; init; }
    public FileStatus Status { get; init; } = FileStatus.Pending;
}
