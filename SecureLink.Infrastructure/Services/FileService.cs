using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using SecureLink.Core;
using SecureLink.Core.Contracts;

// For now I have kept this service here as this assumes our file upload is HTTP based
// As it deals with MultipartReader and MultipartSection
// TODO: Maybe add a new parsing service that handles the parsing of the request
// and we can make this service clean and move it to Core
namespace SecureLink.Infrastructure.Services;

public class FileUploadService(
    IFileRepository repository,
    FileValidator validator,
    ILogger<FileUploadService> logger
) : IFileService
{
    private readonly ILogger<FileUploadService> _logger = logger;
    private readonly IFileRepository _repository = repository;
    private readonly FileValidator _validator = validator;

    public async Task<ServiceResult<string, FileUploadErrorDetails>> Upload(
        string boundary,
        Stream uploadedFileStream
    )
    {
        string outputFileName = Guid.NewGuid().ToString();
        string outputFilePath = "";
        var reader = new MultipartReader(boundary, uploadedFileStream);
        MultipartSection? section;
        long totalBytesRead = 0;

        while ((section = await reader.ReadNextSectionAsync()) != null)
        {
            var contentDispositionHeader = section.GetContentDispositionHeader();
            var mimeType = section.ContentType;

            var reqHeaderValidationResult = _validator.ValidateHeader(contentDispositionHeader);
            if (!reqHeaderValidationResult.IsValid)
            {
                return ServiceResult<string, FileUploadErrorDetails>.ValidationError(
                    reqHeaderValidationResult.Error!
                );
            }

            Stream content = section.Body;

            // If it is a file write it to output file stream
            if (contentDispositionHeader!.IsFileDisposition())
            {
                using var bufferedStream = new FileBufferingReadStream(section.Body, 1024 * 1024);
                byte[] header = new byte[32];
                // Used 32 bytes for peeking
                // As some files like MP4 has its identifying marker slightly offset
                int bytesRead = await bufferedStream.ReadAsync(header.AsMemory(0, 32));

                var originalFileName = contentDispositionHeader!.FileName.Value;
                var fileValidation = _validator.ValidateFile(
                    originalFileName!,
                    mimeType!,
                    header,
                    bytesRead
                );

                if (!fileValidation.IsValid)
                {
                    return ServiceResult<string, FileUploadErrorDetails>.ValidationError(
                        fileValidation.Error!
                    );
                }

                // Reset the stream after peeking
                bufferedStream.Seek(0, SeekOrigin.Begin);

                var extension = Path.GetExtension(originalFileName);
                var finalOutputFileName = Path.ChangeExtension(outputFileName, extension);

                _logger.LogInformation($"Processing file: {originalFileName}");

                outputFilePath = await _repository.Upload(bufferedStream, finalOutputFileName);
                totalBytesRead += content.Length;
            }
            // Else handle the metadata
            else if (contentDispositionHeader!.IsFormDisposition())
            {
                // Converting content from Stream to string
                using var streamReader = new StreamReader(content);
                string value = await streamReader.ReadToEndAsync();
                string key = contentDispositionHeader!.Name.Value ?? "";

                // Just logging for now
                _logger.LogInformation($"Metadata for file: {key} = {value}");
            }
        }

        _logger.LogInformation($"File upload completed. Total bytes read: {totalBytesRead} bytes");

        return ServiceResult<string, FileUploadErrorDetails>.Success(outputFilePath);
    }

    public async Task<ServiceResult<Stream, FileDownloadErrorDetails>> Download(string filename)
    {
        var fileValidation = await _validator.ValidateFileForDownload(filename);
        if (!fileValidation.IsValid)
            return ServiceResult<Stream, FileDownloadErrorDetails>.ValidationError(
                fileValidation.Error!
            );

        try
        {
            var result = await _repository.Download(filename);
            return ServiceResult<Stream, FileDownloadErrorDetails>.Success(result);
        }
        catch (FileNotFoundException)
        {
            return ServiceResult<Stream, FileDownloadErrorDetails>.ValidationError(
                new FileDownloadErrorDetails { Error = "File not found" }
            );
        }
    }
}
