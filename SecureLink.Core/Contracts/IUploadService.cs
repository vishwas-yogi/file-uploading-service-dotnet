namespace SecureLink.Core.Contracts;

public interface IUploadService
{
    public Task<string> Upload(Stream file, string fileName);
    public Task<Stream> Download(string fileName);
    public Task<bool> FileExists(string filePath);
}
