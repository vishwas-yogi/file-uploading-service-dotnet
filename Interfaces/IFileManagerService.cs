namespace FileUploader.Interfaces;

public interface IFileManagerService
{
    public Task<string> UploadFile(string boundary, Stream inputStream);
}
