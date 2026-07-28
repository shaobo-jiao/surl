using Microsoft.EntityFrameworkCore;
using Surl.Api.Common;
using Surl.Api.Data;

namespace Surl.Api.ShortUrls;

public class ShortUrlService(AppDbContext dbContext, IdGenerator<ShortUrl> idGenerator, ILogger<ShortUrlService> logger, TimeProvider timeProvider) : IShortUrlService
{
    private readonly AppDbContext _dbContext = dbContext;
    private readonly IdGenerator<ShortUrl> _idGenerator = idGenerator;
    private readonly ILogger<ShortUrlService> _logger = logger;
    private readonly TimeProvider _timeProvider = timeProvider;

    public async Task<ShortUrl> CreateShortUrlAsync(CreateShortUrlRequest request, int? userId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating ShortUrl for request: {request}, userId: {userId}", request, userId);
        // generate ID and encode code
        var newId = _idGenerator.NewId();
        var shortUrl = new ShortUrl
        {
            Id = newId,
            Code = Base62Encoding.Encode(newId),
            OriginalUrl = request.OriginalUrl,
            ExpiresAt = request.ExpiresAt ?? _timeProvider.GetUtcNow().AddDays(1),
            UserId = userId,
        };

        await _dbContext.ShortUrls.AddAsync(shortUrl, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("ShortUrl created and saved: {id}", shortUrl.Id);
        return shortUrl;
    }

    public async Task<ShortUrl?> GetShortUrlByIdAsync(long shortUrlId, int? userId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting ShortUrl: {id}, userId: {userId}", shortUrlId, userId);
        var shortUrl = await _dbContext.ShortUrls.FindAsync(shortUrlId, cancellationToken);
        
        if (shortUrl is null) return null;
        if (userId.HasValue && shortUrl.UserId != userId) return null; // not creator;
        return shortUrl;
    }

    public async Task<PagedResult<ShortUrl>> GetShortUrlsAsync(QueryShortUrlParams queryParams, int? userId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting ShortUrls with parameter: {queryParams}, userId: {userId}", queryParams, userId);

        var query = _dbContext.ShortUrls.AsQueryable();

        // filter userId;
        if (userId.HasValue) query = query.Where(x => x.UserId == userId);

        // filter param expiry range;
        if (queryParams.ExpiresFrom.HasValue) query = query.Where(x => x.ExpiresAt >= queryParams.ExpiresFrom);
        if (queryParams.ExpiresTo.HasValue) query = query.Where(x => x.ExpiresAt <= queryParams.ExpiresTo);

        // order by
        bool IsDescending = queryParams.IsDescending ?? false;
        query = queryParams.OrderBy switch
        {
            QueryShortUrlAllowedOrderByProperties.ExpiresAt => query.OrderByDirection(x => x.ExpiresAt, IsDescending),
            QueryShortUrlAllowedOrderByProperties.ClickCount => query.OrderByDirection(x => x.Clicks.Count(), IsDescending),
            _ => query.OrderByDirection(x => x.Id, IsDescending), // by default: order by id;
        };
        _logger.LogDebug("GetShortUrls query before pagination: {query}", query.ToQueryString());
        // pagination
        return await query.ToPagedResultAsync(queryParams.Page, queryParams.PageSize, cancellationToken);
    }

    public async Task<Result<ShortUrl, ShortUrlError>> GetRedirectionUrlAsync(string code, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting redirection URL for code: {code}", code);
        if (!Base62Encoding.TryDecode(code, out long id))
            return Result<ShortUrl, ShortUrlError>.Failure(ShortUrlError.InvalidCode);

        _logger.LogDebug("valid code, loading URL from database using id: {id}", id);
        var shortUrl = await _dbContext.ShortUrls.FindAsync(id, cancellationToken);
        if (shortUrl is null)
            return Result<ShortUrl, ShortUrlError>.Failure(ShortUrlError.NotFound);
        
        _logger.LogDebug("shortUrl loaded, checking expiry: {expiresAt}", shortUrl.ExpiresAt);
        if (shortUrl.ExpiresAt < _timeProvider.GetUtcNow())
            return Result<ShortUrl, ShortUrlError>.Failure(ShortUrlError.Expired);

        _logger.LogInformation("returning short url for redirection: {id}", id);
        return Result<ShortUrl, ShortUrlError>.Success(shortUrl);
    }

    public async Task<Result<ShortUrl, ShortUrlError>> UpdateShortUrlAsync(long shortUrlId, UpdateShortUrlRequest request, int? userId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating short url: {id}, userId: {userId}", shortUrlId, userId);
        var shortUrl = await GetShortUrlByIdAsync(shortUrlId, userId, cancellationToken);
        if (shortUrl is null) return Result<ShortUrl, ShortUrlError>.Failure(ShortUrlError.NotFound);

        _logger.LogDebug("Short Url to update found, updating info");
        shortUrl.ExpiresAt = request.ExpiresAt;
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("successfully updated short url: {id}", shortUrlId);
        return Result<ShortUrl, ShortUrlError>.Success(shortUrl);
    }

    public async Task<Result<ShortUrl, ShortUrlError>> DeleteShortUrlAsync(long shortUrlId, int? userId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting Short Url: {id}, userId: {userId}", shortUrlId, userId);
        var shortUrl = await GetShortUrlByIdAsync(shortUrlId, userId, cancellationToken);
        if (shortUrl is null) return Result<ShortUrl, ShortUrlError>.Failure(ShortUrlError.NotFound);
        
        _logger.LogDebug("Short Url to delete found, deleting");
        _dbContext.ShortUrls.Remove(shortUrl);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Successfully removed short url: {id}, userId: {userId}", shortUrlId, userId);
        return Result<ShortUrl, ShortUrlError>.Success(shortUrl);
    }

    /// <summary>
    /// Saves a Short URL click event into database.
    /// </summary>
    /// <param name="clickEvent"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task SaveClickEventAsync(ShortUrlClickEvent clickEvent, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Saving click event : {clickEvent}", clickEvent);
        var click = new ShortUrlClick()
        {
            ShortUrlId = clickEvent.ShortUrlId,
            ClickedAt = clickEvent.ClickedAt
        };
        await _dbContext.ShortUrlClicks.AddAsync(click, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Successfully saved click event: {clickEvent}", clickEvent);
    }
}
