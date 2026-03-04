namespace SecureLink.Core.Contracts;

public interface IThumbnailQueue
{
    ValueTask QueueAsync(ThumbnailJob job);
    ValueTask<ThumbnailJob> DequeueAsync(CancellationToken token);
}
