using FileUploader.Contracts;

namespace FileUploader.Helpers;

public static class FileValidationDefinitions
{
    public static readonly Dictionary<string, FileTypeValidation> AllowedTypes = new(
        StringComparer.OrdinalIgnoreCase
    )
    {
        {
            ".pdf",
            new FileTypeValidation
            {
                Extension = ".pdf",
                MimeType = "application/pdf",
                MagicBytes =
                [
                    [0x25, 0x50, 0x44, 0x46],
                ], // %PDF
            }
        },
        {
            ".png",
            new FileTypeValidation
            {
                Extension = ".png",
                MimeType = "image/png",
                MagicBytes =
                [
                    [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A],
                ],
            }
        },
        {
            ".jpeg",
            new FileTypeValidation
            {
                Extension = ".jpeg",
                MimeType = "image/jpeg",
                MagicBytes =
                [
                    [0xFF, 0xD8, 0xFF, 0xE0], // Standard JPEG
                    [0xFF, 0xD8, 0xFF, 0xE1], // EXIF JPEG
                ],
            }
        },
        {
            ".jpg",
            new FileTypeValidation
            { // Mapping jpg to jpeg rules
                Extension = ".jpg",
                MimeType = "image/jpeg",
                MagicBytes =
                [
                    [0xFF, 0xD8, 0xFF, 0xE0],
                    [0xFF, 0xD8, 0xFF, 0xE1],
                ],
            }
        },
        {
            ".mp4",
            new FileTypeValidation
            {
                Extension = ".mp4",
                MimeType = "video/mp4",
                MagicBytes =
                [
                    [0x00, 0x00, 0x00, 0x18, 0x66, 0x74, 0x79, 0x70], // ftyp...
                    [0x00, 0x00, 0x00, 0x20, 0x66, 0x74, 0x79, 0x70],
                ],
            }
        },
    };
}
