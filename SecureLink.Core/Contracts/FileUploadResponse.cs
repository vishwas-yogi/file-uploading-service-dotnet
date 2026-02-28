namespace SecureLink.Core.Contracts;

public record FileUploadResponse
{
    public Guid? Id { get; set; } = null; // Not required as in case of error, we return null
    public string? Filename { get; set; }
    public bool? IsError { get; set; } = false;
    public FileUploadErrorDetails? Error { get; set; }
}
