using FileUploader.Interfaces;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;

namespace FileUploader.Services;

public class FileManagerService(ILogger<FileManagerService> logger) : IFileManagerService
{
    private const string outputFileName = "test.txt";
    private readonly ILogger<FileManagerService> _logger = logger;

    public async Task<string> UploadFile(string boundary, Stream uploadedFileStream)
    {
        // Construct file path and delete the file if it already exists
        string outputFilePath = Path.Combine(Directory.GetCurrentDirectory(), outputFileName);
        RemoveFileIfExists(outputFilePath);

        // Constuct FileStream for the output file
        var options = new FileStreamOptions
        {
            Mode = FileMode.Create,
            Access = FileAccess.Write,
            Options = FileOptions.Asynchronous,
            Share = FileShare.None,
        };

        using FileStream outputStream = new(outputFilePath, options);

        var reader = new MultipartReader(boundary, uploadedFileStream);
        MultipartSection? section;
        long totalBytesRead = 0;

        while ((section = await reader.ReadNextSectionAsync()) != null)
        {
            var contentDispositionHeader = section.GetContentDispositionHeader();

            if (contentDispositionHeader == null)
            {
                _logger.LogError("The request must contain the content disposition header");
                throw new InvalidOperationException(
                    "The request must contain the content disposition header"
                );
            }

            Stream content = section.Body;

            // If it is a file write it to output file stream
            if (contentDispositionHeader.IsFileDisposition())
            {
                _logger.LogInformation(
                    $"Processing file: ${contentDispositionHeader.FileName.Value}"
                );

                await content.CopyToAsync(outputStream);
                totalBytesRead += content.Length;
            }
            // Else handle the metadata
            else if (contentDispositionHeader.IsFormDisposition())
            {
                // Converting content from Stream to string
                using var streamReader = new StreamReader(content);
                string value = await streamReader.ReadToEndAsync();
                string key = contentDispositionHeader.Name.Value ?? "";

                // Just logging for now
                _logger.LogInformation($"Metadata for file: {key} = {value}");
            }
        }

        _logger.LogInformation($"File upload completed. Total bytes read: {totalBytesRead} bytes");

        return outputFilePath;
    }

    private void RemoveFileIfExists(string filePath)
    {
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
            _logger.LogInformation($"Deleted file: {filePath}");
        }
    }
}
