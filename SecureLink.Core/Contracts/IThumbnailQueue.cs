namespace SecureLink.Core.Contracts;

public interface IThumbnailQueue
{
    ValueTask QueueAsync(ThumbnailJob job, CancellationToken token = default);
    ValueTask<ThumbnailJob> DequeueAsync(CancellationToken token = default);
}
