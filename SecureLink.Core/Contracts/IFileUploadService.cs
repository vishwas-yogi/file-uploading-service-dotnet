namespace SecureLink.Core.Contracts;

public interface IFileUploadService
{
    public Task<ServiceResult<string, FileUploadErrorDetails>> UploadFile(
        string boundary,
        Stream inputStream
    );
}
