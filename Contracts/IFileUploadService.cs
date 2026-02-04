namespace SecureLink.Contracts;

public interface IFileUploadService
{
    public Task<ServiceResult<string, FileUploadErrorDetails>> UploadFile(
        string boundary,
        Stream inputStream
    );
}
