namespace SecureLink.Core.Contracts;

public record FileDownloadServiceResponse
{
    public FileResponse? FileDetails { get; init; }

    /// <summary>
    ///  Take care of disposing it.
    /// If is not being disposed automatically.
    /// Using File() in the controller automatically handles the dispositon
    /// </summary>
    public Stream? FileStream { get; init; }
}
