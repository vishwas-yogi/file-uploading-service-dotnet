namespace SecureLink.Core.Contracts;

public interface IFileService
{
    public Task<ServiceResult<string, FileUploadErrorDetails>> Upload(
        string boundary,
        Stream inputStream
    );
    public Task<ServiceResult<Stream, FileDownloadErrorDetails>> Download(string filename);
}
