namespace Surl.Api.ShortUrls;

/// <summary>
/// Represents a background service that consumes click events from a in-memory queue
/// and saves the click events into database. 
/// Currently each click event triggers a scope with DbContext creation and disposal.
/// Later we may consider batch processing. 
/// </summary>
public class ShortUrlClickBackgroundService(IServiceScopeFactory scopeFactory, ShortUrlClickEventQueue eventQueue,
    ILogger<ShortUrlClickBackgroundService> logger) : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
    private readonly ShortUrlClickEventQueue _clickQueue = eventQueue;
    private readonly ILogger<ShortUrlClickBackgroundService> _logger = logger;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Short URL click processor started");
        try
        {
            // async enumerable: each time load a click event from the queue, or wait if it's empty.
            await foreach (var clickEvent in _clickQueue.DequeueAllAsync(stoppingToken))
            {
                _logger.LogDebug("Processing click event for short URL {ShortUrlId} clicked at {ClickedAt}",
                    clickEvent.ShortUrlId, clickEvent.ClickedAt);

                try
                {
                    await using var scope = _scopeFactory.CreateAsyncScope();
                    var shortUrlService = scope.ServiceProvider.GetRequiredService<IShortUrlService>();
                    await shortUrlService.SaveClickEventAsync(clickEvent, stoppingToken);

                    _logger.LogDebug("Click event processed for short URL {ShortUrlId}", clickEvent.ShortUrlId);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    throw; // cancellation requested, terminate
                }
                catch (Exception ex)
                {
                    // The in-memory event is lost after this failure. Log it and briefly
                    // back off before processing the next event.
                    _logger.LogError(ex, "Click event processing failed for short URL {ShortUrlId}; event will be discarded",
                        clickEvent.ShortUrlId);
                    await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
                }
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // Expected during application shutdown.
        }
        finally
        {
            _logger.LogInformation("Short URL click processor stopped");
        }
    }
}
