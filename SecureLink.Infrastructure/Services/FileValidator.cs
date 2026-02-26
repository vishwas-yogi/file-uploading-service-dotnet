using Microsoft.Net.Http.Headers;
using SecureLink.Core;
using SecureLink.Core.Contracts;
using static SecureLink.Core.Helpers.FileValidationDefinitions;

namespace SecureLink.Infrastructure.Services;

public class FileValidator(IFileRepository fileRepository)
{
    private readonly IFileRepository _repository = fileRepository;

    public ValidationResult<FileUploadErrorDetails> ValidateFile(
        string? fileName,
        string mimeType,
        byte[] fileInitialBytes,
        int bytesRead
    )
    {
        // TODO: In future, we can change method to accept rules
        // then we can iterate over the rules instead of manually calling each validation
        var fileNameValidationRes = ValidateFileName(fileName);
        if (!fileNameValidationRes.IsValid)
        {
            return fileNameValidationRes;
        }

        var fileValidationRes = ValidateFileInternal(
            fileName!,
            mimeType,
            fileInitialBytes,
            bytesRead
        );
        if (!fileValidationRes.IsValid)
        {
            return fileValidationRes;
        }

        return new ValidationResult<FileUploadErrorDetails> { IsValid = true };
    }

    public ValidationResult<FileUploadErrorDetails> ValidateHeader(
        ContentDispositionHeaderValue? header
    )
    {
        if (header == null)
        {
            return new ValidationResult<FileUploadErrorDetails>
            {
                IsValid = false,
                Error = new FileUploadErrorDetails
                {
                    Message = "Invalid request, header can't be empty",
                },
            };
        }
        return new ValidationResult<FileUploadErrorDetails> { IsValid = true };
    }

    public static ValidationResult<FileUploadErrorDetails> ValidateMetaData()
    {
        // TODO: Add some metadata validation
        // I couldn't think of any metadata validation for now
        // I'll add it later
        return new ValidationResult<FileUploadErrorDetails> { IsValid = true };
    }

    public async Task<ValidationResult<FileDownloadErrorDetails>> ValidateFileForDownload(
        string filename
    )
    {
        var fileExists = await _repository.FileExists(filename);

        if (!fileExists)
        {
            return new ValidationResult<FileDownloadErrorDetails>
            {
                IsValid = false,
                Error = new FileDownloadErrorDetails { Error = $"File '{filename}' not found" },
            };
        }
        return new ValidationResult<FileDownloadErrorDetails> { IsValid = true };
    }

    private static ValidationResult<FileUploadErrorDetails> ValidateFileName(string? filename)
    {
        if (string.IsNullOrWhiteSpace(filename))
        {
            return new ValidationResult<FileUploadErrorDetails>
            {
                IsValid = false,
                Error = new FileUploadErrorDetails
                {
                    Message = "Filename must be provided for uploading",
                },
            };
        }
        return new ValidationResult<FileUploadErrorDetails> { IsValid = true };
    }

    private static ValidationResult<FileUploadErrorDetails> ValidateFileInternal(
        string filename,
        string mimeType,
        byte[] fileInitialBytes,
        int bytesRead
    )
    {
        // Check for allowed file extensions
        string extension = Path.GetExtension(filename);
        if (
            string.IsNullOrEmpty(extension)
            || !AllowedTypes.TryGetValue(extension, out var matchedExtension)
        )
        {
            return new ValidationResult<FileUploadErrorDetails>
            {
                IsValid = false,
                Error = new FileUploadErrorDetails
                {
                    Message = $"Unsupported file type {extension} for '{filename}'",
                },
            };
        }

        // Check for correct MIME types
        if (
            string.IsNullOrEmpty(mimeType)
            || !string.Equals(
                mimeType,
                matchedExtension.MimeType,
                StringComparison.OrdinalIgnoreCase
            )
        )
        {
            return new ValidationResult<FileUploadErrorDetails>
            {
                IsValid = false,
                Error = new FileUploadErrorDetails
                {
                    Message = $"Invalid / Unsupported MIME type '{mimeType}' for file '{filename}'",
                },
            };
        }

        // Check for correct file signature
        if (bytesRead < 4)
        {
            return new ValidationResult<FileUploadErrorDetails>
            {
                IsValid = false,
                Error = new FileUploadErrorDetails
                {
                    Message = $"File '{filename}' is too small to be a valid file.",
                },
            };
        }

        if (matchedExtension.MagicBytes.Length > 0)
        {
            bool matches = matchedExtension.MagicBytes.Any(signature =>
            {
                if (bytesRead < signature.Length)
                    return false;
                return fileInitialBytes.AsSpan(0, signature.Length).SequenceEqual(signature);
            });

            if (!matches)
            {
                return new ValidationResult<FileUploadErrorDetails>
                {
                    IsValid = false,
                    Error = new FileUploadErrorDetails
                    {
                        Message = $"Invalid file signature for file '{filename}'",
                    },
                };
            }
        }

        return new ValidationResult<FileUploadErrorDetails> { IsValid = true };
    }
}
