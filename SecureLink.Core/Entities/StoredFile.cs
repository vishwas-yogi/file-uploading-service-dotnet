namespace SecureLink.Core.Entities;

public class StoredFile
{
    public required Guid Id { get; set; }
    public required string Filename { get; set; }
    public required string UserFilename { get; set; }
    public required string ContentType { get; set; }
    public required string Location { get; set; }
    public required Guid Owner { get; set; }
    public string Metadata { get; set; } = "{}";
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? LastModifiedAt { get; set; }
}
