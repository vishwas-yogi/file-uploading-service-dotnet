namespace SecureLink.Core.Contracts;

public interface IFilesService
{
    public Task<ServiceResult<List<FileUploadResponse>, FileUploadErrorDetails>> Upload(
        string boundary,
        Stream inputStream,
        Guid currentUser
    );
    public Task<ServiceResult<FileDownloadServiceResponse, FileDownloadErrorDetails>> Download(
        Guid fileId,
        Guid currentUserId
    );
}
