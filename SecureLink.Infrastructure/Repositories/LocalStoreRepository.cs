using Microsoft.Extensions.Logging;
using SecureLink.Core.Contracts;

namespace SecureLink.Infrastructure.Repositories;

public class LocalStoreRepository(ILogger<LocalStoreRepository> logger) : IFileUploadRepository
{
    private readonly ILogger<LocalStoreRepository> _logger = logger;

    public async Task<string> UploadFile(Stream file, string fileName)
    {
        var outputFilePath = Path.Combine(GetOutputDir(), fileName);
        RemoveFileIfExists(outputFilePath);

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

    private void RemoveFileIfExists(string filePath)
    {
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
            _logger.LogInformation($"Deleted file: {filePath}");
        }
    }

    private static string GetOutputDir()
    {
        string outDir = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
        if (!Directory.Exists(outDir))
        {
            Directory.CreateDirectory(outDir);
        }
        return outDir;
    }
}
