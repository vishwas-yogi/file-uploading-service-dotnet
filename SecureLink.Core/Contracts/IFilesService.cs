namespace SecureLink.Core.Contracts;

public interface IFilesService
{
    public Task<ServiceResult<List<string>, FileUploadErrorDetails>> Upload(
        string boundary,
        Stream inputStream,
        Guid currentUser
    );
    public Task<ServiceResult<Stream, FileDownloadErrorDetails>> Download(string filename);
}
