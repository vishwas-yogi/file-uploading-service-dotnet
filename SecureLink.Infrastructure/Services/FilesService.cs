using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using SecureLink.Core;
using SecureLink.Core.Contracts;
using SecureLink.Infrastructure.Contracts;
using SecureLink.Infrastructure.Helpers;

namespace SecureLink.Infrastructure.Services;

public class FilesService(
    IStorageService storageService,
    FilesValidator validator,
    IFilesRepository filesRepository,
    ILogger<FilesService> logger
) : IFilesService
{
    private readonly ILogger<FilesService> _logger = logger;
    private readonly IStorageService _storageService = storageService;
    private readonly FilesValidator _validator = validator;
    private readonly IFilesRepository _filesRepository = filesRepository;

    public async Task<ServiceResult<List<FileUploadResponse>, FileUploadErrorDetails>> Upload(
        string boundary,
        Stream uploadedFileStream,
        Guid currentUser
    )
    {
        List<FileUploadResponse> results = [];
        var reader = new MultipartReader(boundary, uploadedFileStream);
        MultipartSection? section;
        long totalBytesRead = 0;

        while ((section = await reader.ReadNextSectionAsync()) != null)
        {
            var contentDispositionHeader = section.GetContentDispositionHeader();
            var contentType = section.ContentType;
            var response = new FileUploadResponse();

            var reqHeaderValidationResult = _validator.ValidateHeader(contentDispositionHeader);
            if (!reqHeaderValidationResult.IsValid)
            {
                response.Error = reqHeaderValidationResult.Error;
                response.IsError = true;
                results.Add(response);
                continue;
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
                response.Filename = originalFileName;
                var fileValidation = _validator.ValidateFile(
                    originalFileName,
                    contentType,
                    header,
                    bytesRead
                );

                if (!fileValidation.IsValid)
                {
                    response.IsError = true;
                    response.Error = fileValidation.Error;
                    results.Add(response);
                    continue;
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
                {
                    response.IsError = true;
                    response.Error = result.Error;
                    results.Add(response);
                    continue;
                }

                response.Id = result.Data;
                results.Add(response);
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

        return ServiceResult<List<FileUploadResponse>, FileUploadErrorDetails>.Success(results);
    }

    public async Task<
        ServiceResult<FileDownloadServiceResponse, FileDownloadErrorDetails>
    > Download(Guid fileId, Guid currentUserId)
    {
        var file = await _filesRepository.Get(
            new FileGetRepoRequest { Id = fileId, Owner = currentUserId }
        );

        if (file is null || file.Status != FileStatus.Available)
        {
            return ServiceResult<FileDownloadServiceResponse, FileDownloadErrorDetails>.NotFound(
                new FileDownloadErrorDetails { Error = "Unable to find requested file" }
            );
        }

        var fileValidation = await _validator.ValidateFileForDownload(file.Filename);
        // Although it is in validator for now, but it checks the storage existenece
        // So instead of retuning validation error, returning 404 for now
        if (!fileValidation.IsValid)
            return ServiceResult<FileDownloadServiceResponse, FileDownloadErrorDetails>.NotFound(
                fileValidation.Error!
            );

        try
        {
            var fileStream = await _storageService.Download(file.Filename);
            return ServiceResult<FileDownloadServiceResponse, FileDownloadErrorDetails>.Success(
                new FileDownloadServiceResponse
                {
                    FileDetails = file.ToDto(),
                    FileStream = fileStream,
                }
            );
        }
        catch (FileNotFoundException)
        {
            return ServiceResult<FileDownloadServiceResponse, FileDownloadErrorDetails>.NotFound(
                new FileDownloadErrorDetails { Error = "File not found" }
            );
        }
    }

    private async Task<ServiceResult<Guid, FileUploadErrorDetails>> Persist(
        FilePersistInternalRequest request
    )
    {
        try
        {
            Guid fileId = await _filesRepository.Persist(request.RepoRequest);
            if (fileId == Guid.Empty)
                return ServiceResult<Guid, FileUploadErrorDetails>.UnexpectedError(
                    new FileUploadErrorDetails
                    {
                        Message = "Failed to initialize file record in the db.",
                    }
                );

            string uplaodedFileLocation = await _storageService.Upload(
                request.UploadedFileStream,
                request.RepoRequest.Filename
            );
            if (string.IsNullOrEmpty(uplaodedFileLocation))
                return ServiceResult<Guid, FileUploadErrorDetails>.UnexpectedError(
                    new FileUploadErrorDetails
                    {
                        Message = "Failed to upload file to storage service ",
                    }
                );

            var updated = await _filesRepository.MarkFileAvailable(fileId, uplaodedFileLocation);
            if (!updated)
                return ServiceResult<Guid, FileUploadErrorDetails>.UnexpectedError(
                    new FileUploadErrorDetails { Message = "Failed to mark the file as available" }
                );

            return ServiceResult<Guid, FileUploadErrorDetails>.Success(fileId);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Unexpected error while uploading file {filename}",
                request.RepoRequest.Filename
            );
            return ServiceResult<Guid, FileUploadErrorDetails>.UnexpectedError(
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
