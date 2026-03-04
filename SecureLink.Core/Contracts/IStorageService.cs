namespace SecureLink.Core.Contracts;

public interface IStorageService
{
    public Task<string> Upload(Stream file, string storageKey);
    public Task<Stream> Download(string storageKey);
    public Task<bool> FileExists(string storageKey);
}
