using System.Threading.Channels;

namespace Surl.Api.ShortUrls;

/// <summary>
/// Represents a queue that wraps around Channel and supports Producer-Consumer pattern. 
/// Instead of immediately saving a click event when a short url is visited, we enqueue
/// the click event and delegate the saving operation to a background service.
/// </summary>
public class ShortUrlClickEventQueue
{
    private readonly Channel<ShortUrlClickEvent> _channel;

    public ShortUrlClickEventQueue()
    {
        // bounded with 10_000 capacity. wait when adding a new event but the queue is full;
        _channel = Channel.CreateBounded<ShortUrlClickEvent>(new BoundedChannelOptions(10_000)
        {
            FullMode = BoundedChannelFullMode.Wait, 
        });
    }

    public async Task EnqueueAsync(ShortUrlClickEvent clickEvent, CancellationToken cancellationToken)
    {
        await _channel.Writer.WriteAsync(clickEvent, cancellationToken);
    }

    public IAsyncEnumerable<ShortUrlClickEvent> DequeueAllAsync(CancellationToken cancellationToken)
    {
        return _channel.Reader.ReadAllAsync(cancellationToken);
    }
}
