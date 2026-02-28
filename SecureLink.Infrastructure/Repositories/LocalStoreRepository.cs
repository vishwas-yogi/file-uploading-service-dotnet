using Microsoft.Extensions.Logging;
using SecureLink.Core.Contracts;

namespace SecureLink.Infrastructure.Repositories;

public class LocalStoreRepository(ILogger<LocalStoreRepository> logger) : IUploadService
{
    private readonly ILogger<LocalStoreRepository> _logger = logger;

    public async Task<string> Upload(Stream file, string fileName)
    {
        var outputFilePath = GetFullFilePath(fileName);
        await RemoveFileIfExists(outputFilePath);

        // Constuct FileStream for the output file
        var options = new FileStreamOptions
        {
            Mode = FileMode.Create,
            Access = FileAccess.Write,
            Options = FileOptions.Asynchronous,
            Share = FileShare.None,
        };

        using FileStream outputStream = new(outputFilePath, options);

        await file.CopyToAsync(outputStream);
        return outputFilePath;
    }

    public Task<Stream> Download(string fileName)
    {
        _logger.LogInformation("Starting download of the requested file");
        var filePath = GetFullFilePath(fileName);

        var options = new FileStreamOptions
        {
            Mode = FileMode.Open,
            Access = FileAccess.Read,
            Options = FileOptions.Asynchronous,
            Share = FileShare.Read, // So that mutiple downloads can happen parallely
        };

        var downloadStream = new FileStream(filePath, options);
        return Task.FromResult<Stream>(downloadStream);
    }

    public async Task<bool> FileExists(string filename)
    {
        var filePath = GetFullFilePath(filename);
        if (await FileExistsInternal(filePath))
            return true;

        return false;
    }

    private Task<bool> FileExistsInternal(string filePath)
    {
        if (File.Exists(filePath))
            return Task.FromResult(true);

        return Task.FromResult(false);
    }

    private async Task RemoveFileIfExists(string filePath)
    {
        if (await FileExistsInternal(filePath))
        {
            File.Delete(filePath);
            _logger.LogInformation($"Deleted file: {filePath}");
        }
    }

    private static string GetOutputDir()
    {
        string outDir = Path.Combine("/home/vishwas-yogi/personal", "uploads");
        if (!Directory.Exists(outDir))
        {
            Directory.CreateDirectory(outDir);
        }
        return outDir;
    }

    private static string GetFullFilePath(string filename)
    {
        return Path.Combine(GetOutputDir(), filename);
    }
}
