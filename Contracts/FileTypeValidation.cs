namespace FileUploader.Contracts;

public class FileTypeValidation
{
    public required string Extension { get; set; }
    public required string MimeType { get; set; }
    public required byte[][] MagicBytes { get; set; } // Nested array because some types (like JPEG) have multiple valid headers
}
