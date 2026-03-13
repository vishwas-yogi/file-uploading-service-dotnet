using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using SecureLink.Core;
using SecureLink.Core.Contracts;
using SecureLink.Infrastructure.BackgroundServices.ThumbnailGenerationJob;
using SecureLink.Infrastructure.Contracts;
using SecureLink.Infrastructure.Helpers;

namespace SecureLink.Infrastructure.Services;

public class FilesService(
    IStorageService storageService,
    FilesValidator validator,
    IFilesRepository filesRepository,
    IThumbnailQueue thumbnailQueue,
    ILogger<FilesService> logger
) : IFilesService
{
    private readonly ILogger<FilesService> _logger = logger;
    private readonly IStorageService _storageService = storageService;
    private readonly FilesValidator _validator = validator;
    private readonly IFilesRepository _filesRepository = filesRepository;
    private readonly IThumbnailQueue _thumbnailQueue = thumbnailQueue;

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

                // Persist the file to the storage.
                var result = await Persist(
                    new FilePersistInternalRequest
                    {
                        RepoRequest = repoRequest,
                        FileStream = bufferedStream,
                    }
                );

                if (!result.IsSuccess)
                {
                    response.IsError = true;
                    response.Error = result.Error;
                    results.Add(response);
                    continue;
                }

                var fileId = result.Data!.FileId;
                var storageKey = result.Data.StorageKey;

                response.Id = fileId;
                results.Add(response);

                // As the response is successful,
                // Add the file to the queue for thumbnail generation
                await AddThumbnailJob(
                    new ThumbnailJob
                    {
                        FileId = fileId,
                        Filename = finalOutputFileName,
                        StorageKey = storageKey,
                    }
                );

                if (bufferedStream.CanSeek)
                {
                    totalBytesRead += bufferedStream.Length;
                }
            }
            // Else handle the metadata
            else if (contentDispositionHeader!.IsFormDisposition())
            {
                // Converting content from Stream to string
                using var streamReader = new StreamReader(content);
                string value = await streamReader.ReadToEndAsync();
                string prop = contentDispositionHeader!.Name.Value ?? "";

                // Just logging for now
                _logger.LogInformation("Metadata for file: {key} = {value}", prop, value);
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

    private async Task<ServiceResult<FilePersistInternalResponse, FileUploadErrorDetails>> Persist(
        FilePersistInternalRequest request
    )
    {
        try
        {
            Guid fileId = await _filesRepository.Persist(request.RepoRequest);
            if (fileId == Guid.Empty)
                return ServiceResult<
                    FilePersistInternalResponse,
                    FileUploadErrorDetails
                >.UnexpectedError(
                    new FileUploadErrorDetails
                    {
                        Message = "Failed to initialize file record in the db.",
                    }
                );

            string storedKey = await _storageService.Upload(
                request.FileStream,
                request.RepoRequest.Filename
            );
            if (string.IsNullOrEmpty(storedKey))
                return ServiceResult<
                    FilePersistInternalResponse,
                    FileUploadErrorDetails
                >.UnexpectedError(
                    new FileUploadErrorDetails
                    {
                        Message = "Failed to upload file to storage service ",
                    }
                );

            var updated = await _filesRepository.MarkFileAvailable(fileId, storedKey);
            // File remains in Pending state; a background cleanup job will
            // remove files stuck in Pending beyond the configured threshold.
            if (!updated)
                return ServiceResult<
                    FilePersistInternalResponse,
                    FileUploadErrorDetails
                >.UnexpectedError(
                    new FileUploadErrorDetails { Message = "Failed to mark the file as available" }
                );

            return ServiceResult<FilePersistInternalResponse, FileUploadErrorDetails>.Success(
                new FilePersistInternalResponse { FileId = fileId, StorageKey = storedKey }
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Unexpected error while uploading file {filename}",
                request.RepoRequest.Filename
            );
            return ServiceResult<
                FilePersistInternalResponse,
                FileUploadErrorDetails
            >.UnexpectedError(
                new FileUploadErrorDetails
                {
                    Message =
                        $"Something went wrong while uplaoding file: {request.RepoRequest.UserFilename}",
                }
            );
        }
    }

    private async Task AddThumbnailJob(ThumbnailJob job)
    {
        try
        {
            await _thumbnailQueue.QueueAsync(job);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to enqueue thumbnail job for fileId: {fileId}",
                job.FileId
            );
        }
    }

    private record FilePersistInternalRequest
    {
        public required FilePersistRepoRequest RepoRequest { get; init; }
        public required Stream FileStream { get; init; }
    }

    private record FilePersistInternalResponse
    {
        public required Guid FileId { get; init; }
        public required string StorageKey { get; init; }
    }
}
