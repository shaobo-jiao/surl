using System.Threading.Channels;
using Namotion.Reflection;

namespace Surl.Api;

/// <summary>
/// Represents a queue that wraps around Channel and supports Producer-Consumer pattern. 
/// Instead of immediately saving a click event when a short url is visited, we enqueue
/// the click event and delegate the saving opreation to a background serivce.
/// </summary>
public class ShortUrlClickEventQueue
{
    private readonly Channel<ShortUrlClickEvent> _channel;

    public ShortUrlClickEventQueue()
    {
        // bounded with 10_000 capatity. wait when adding a new event but the queue is full;
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
