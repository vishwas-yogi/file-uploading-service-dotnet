namespace SecureLink.Core.Contracts;

public record FileDownloadServiceResponse
{
    public FileResponse? FileDetails { get; init; }
    public Stream? FileStream { get; init; }
}
