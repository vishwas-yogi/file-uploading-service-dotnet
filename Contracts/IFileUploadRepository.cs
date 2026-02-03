namespace FileUploader.Contracts;

public interface IFileUploadRepository
{
    public Task<string> UploadFile(Stream file, string fileName);
}
