namespace Surl.Api.ShortUrls;

/// <summary>
/// Represents a background service that consumes click events from a in-memory queue
/// and saves the click events into database. 
/// Currently each click event triggers a scope with DbContext creation and disposal.
/// Later we may consider batch processing. 
/// </summary>
public class ShortUrlClickBackgroundService(
    IServiceScopeFactory scopeFactory,
    ShortUrlClickEventQueue eventQueue,
    ILogger<ShortUrlClickBackgroundService> logger) : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
    private readonly ShortUrlClickEventQueue _clickQueue = eventQueue;
    private readonly ILogger<ShortUrlClickBackgroundService> _logger = logger;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ShortUrlClickBackgroundService is starting.");
        try
        {
            // async enumerable: each time load a click event from the queue, or wait if it's empty.
            await foreach (var clickEvent in _clickQueue.DequeueAllAsync(stoppingToken))
            {
                _logger.LogInformation("Processing click event: {ClickEvent}", clickEvent);

                try
                {
                    await using var scope = _scopeFactory.CreateAsyncScope();
                    var shortUrlService = scope.ServiceProvider.GetRequiredService<ShortUrlService>();
                    await shortUrlService.SaveClickEventAsync(clickEvent, stoppingToken);

                    _logger.LogInformation("Successfully processed click event: {ClickEvent}", clickEvent);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    throw; // cancellation requested, terminate
                }
                catch (Exception ex)
                {
                    // log unexpected exception, sleep and continue;
                    _logger.LogError(ex, "Failed to process click event: {ClickEvent} ", clickEvent);
                    await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
                }
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // operation cancelled, log data and omit the exception;
            _logger.LogInformation("ShortUrlClickBackgroundService is stopping.");
        }
    }
}
