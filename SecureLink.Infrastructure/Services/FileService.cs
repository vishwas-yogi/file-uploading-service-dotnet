using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using SecureLink.Core;
using SecureLink.Core.Contracts;
using SecureLink.Infrastructure.Contracts;
using SecureLink.Infrastructure.Repositories;

namespace SecureLink.Infrastructure.Services;

public class FilesService(
    IFileRepository repository,
    FileValidator validator,
    FileRepository fileRepository,
    ILogger<FilesService> logger
) : IFilesService
{
    private readonly ILogger<FilesService> _logger = logger;
    private readonly IFileRepository _repository = repository;
    private readonly FileValidator _validator = validator;
    private readonly FileRepository _fileRepository = fileRepository;

    public async Task<ServiceResult<List<string>, FileUploadErrorDetails>> Upload(
        string boundary,
        Stream uploadedFileStream,
        Guid currentUser
    )
    {
        List<string> outputFilePaths = [];
        var reader = new MultipartReader(boundary, uploadedFileStream);
        MultipartSection? section;
        long totalBytesRead = 0;

        while ((section = await reader.ReadNextSectionAsync()) != null)
        {
            var contentDispositionHeader = section.GetContentDispositionHeader();
            var contentType = section.ContentType;

            var reqHeaderValidationResult = _validator.ValidateHeader(contentDispositionHeader);
            if (!reqHeaderValidationResult.IsValid)
            {
                return ServiceResult<List<string>, FileUploadErrorDetails>.ValidationError(
                    reqHeaderValidationResult.Error!
                );
            }

            Stream content = section.Body;

            // If it is a file write it to output file stream
            if (contentDispositionHeader!.IsFileDisposition())
            {
                string outputFileName = Guid.NewGuid().ToString();
                using var bufferedStream = new FileBufferingReadStream(section.Body, 1024 * 1024);
                byte[] header = new byte[32];
                // Used 32 bytes for peeking
                // As some files like MP4 has its identifying marker slightly offset
                int bytesRead = await bufferedStream.ReadAsync(header.AsMemory(0, 32));

                var originalFileName = contentDispositionHeader!.FileName.Value;
                var fileValidation = _validator.ValidateFile(
                    originalFileName!,
                    contentType!,
                    header,
                    bytesRead
                );

                if (!fileValidation.IsValid)
                {
                    return ServiceResult<List<string>, FileUploadErrorDetails>.ValidationError(
                        fileValidation.Error!
                    );
                }

                // Reset the stream after peeking
                bufferedStream.Seek(0, SeekOrigin.Begin);

                var extension = Path.GetExtension(originalFileName);
                var finalOutputFileName = Path.ChangeExtension(outputFileName, extension);

                _logger.LogInformation("Processing file: {originalFileName}", originalFileName);

                var repoRequest = new FilePersistRepoRequest
                {
                    Filename = finalOutputFileName,
                    UserFilename = originalFileName!,
                    Owner = currentUser,
                    ContentType = contentType!,
                };

                var result = await Persist(
                    new FilePersistInternalRequest
                    {
                        RepoRequest = repoRequest,
                        UploadedFileStream = bufferedStream,
                    }
                );

                if (!result.IsSuccess)
                    return ServiceResult<List<string>, FileUploadErrorDetails>.UnexpectedError(
                        result.Error!
                    );

                outputFilePaths.Add(result.Data!);
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
                _logger.LogInformation("Metadata for file: {key} = {value}", key, value);
            }
        }

        _logger.LogInformation(
            "File upload completed. Total bytes read: {totalBytesRead} bytes",
            totalBytesRead
        );

        return ServiceResult<List<string>, FileUploadErrorDetails>.Success(outputFilePaths);
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

    private async Task<ServiceResult<string, FileUploadErrorDetails>> Persist(
        FilePersistInternalRequest request
    )
    {
        try
        {
            Guid fileId = await _fileRepository.Persist(request.RepoRequest);
            if (fileId == Guid.Empty)
                return ServiceResult<string, FileUploadErrorDetails>.UnexpectedError(
                    new FileUploadErrorDetails
                    {
                        Message = "Failed to initialize file record in the db.",
                    }
                );

            string uplaodedFileLocation = await _repository.Upload(
                request.UploadedFileStream,
                request.RepoRequest.Filename
            );
            if (string.IsNullOrEmpty(uplaodedFileLocation))
                return ServiceResult<string, FileUploadErrorDetails>.UnexpectedError(
                    new FileUploadErrorDetails
                    {
                        Message = "Failed to upload file to storage service ",
                    }
                );

            var updated = await _fileRepository.MarkFileAvailable(fileId, uplaodedFileLocation);
            if (!updated)
                return ServiceResult<string, FileUploadErrorDetails>.UnexpectedError(
                    new FileUploadErrorDetails { Message = "Failed to mark the file as available" }
                );

            return ServiceResult<string, FileUploadErrorDetails>.Success(uplaodedFileLocation);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Unexpected error while uploading file {filename}",
                request.RepoRequest.Filename
            );
            return ServiceResult<string, FileUploadErrorDetails>.UnexpectedError(
                new FileUploadErrorDetails
                {
                    Message =
                        $"Something went wrong while uplaoding file: {request.RepoRequest.UserFilename}",
                }
            );
        }
    }

    private record FilePersistInternalRequest
    {
        public required FilePersistRepoRequest RepoRequest { get; init; }
        public required Stream UploadedFileStream { get; init; }
    }
}
