using System.Threading.Channels;
using SecureLink.Core.Contracts;

namespace SecureLink.Infrastructure.BackgroundServices.ThumbnailGenerationJob;

public class ThumbnailQueue : IThumbnailQueue
{
    private const int _capacity = 50;
    private readonly Channel<ThumbnailJob> _channel;

    public ThumbnailQueue()
    {
        _channel = Channel.CreateBounded<ThumbnailJob>(
            new BoundedChannelOptions(_capacity) { FullMode = BoundedChannelFullMode.Wait }
        );
    }

    public async ValueTask<ThumbnailJob> DequeueAsync(CancellationToken token)
    {
        return await _channel.Reader.ReadAsync(token);
    }

    public async ValueTask QueueAsync(ThumbnailJob job)
    {
        await _channel.Writer.WriteAsync(job);
    }
}
