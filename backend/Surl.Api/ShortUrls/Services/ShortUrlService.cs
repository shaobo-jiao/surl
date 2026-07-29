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
        _logger.LogDebug("Creating short URL for user {UserId}", userId);
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

        _logger.LogInformation("Short URL {ShortUrlId} created for user {UserId}", shortUrl.Id, userId);
        return shortUrl;
    }

    public async Task<ShortUrl?> GetShortUrlByIdAsync(long shortUrlId, int userId, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Getting short URL {ShortUrlId} for user {UserId}", shortUrlId, userId);
        var shortUrl = await _dbContext.ShortUrls
            .FirstOrDefaultAsync(x => x.Id == shortUrlId && x.UserId == userId, cancellationToken);

        if (shortUrl is null)
            _logger.LogDebug("Short URL {ShortUrlId} was not found for user {UserId}", shortUrlId, userId);

        return shortUrl;
    }

    public async Task<PagedResult<ShortUrl>> GetShortUrlsAsync(QueryShortUrlParams queryParams, int userId, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Getting short URLs page {Page} with page size {PageSize} for user {UserId}",
            queryParams.Page, queryParams.PageSize, userId);

        var query = _dbContext.ShortUrls.AsQueryable();

        // filter userId;
        query = query.Where(x => x.UserId == userId);

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
        // pagination
        return await query.ToPagedResultAsync(queryParams.Page, queryParams.PageSize, cancellationToken);
    }

    public async Task<Result<ShortUrl, ShortUrlError>> GetRedirectionUrlAsync(string code, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Resolving short URL code");
        if (!Base62Encoding.TryDecode(code, out long id))
        {
            _logger.LogDebug("Short URL resolution rejected because the code is invalid");
            return Result<ShortUrl, ShortUrlError>.Failure(ShortUrlError.InvalidCode);
        }

        _logger.LogDebug("Loading short URL {ShortUrlId}", id);
        var shortUrl = await _dbContext.ShortUrls.FindAsync(id, cancellationToken);
        if (shortUrl is null)
        {
            _logger.LogDebug("Short URL {ShortUrlId} was not found", id);
            return Result<ShortUrl, ShortUrlError>.Failure(ShortUrlError.NotFound);
        }
        
        if (shortUrl.ExpiresAt <= _timeProvider.GetUtcNow())
        {
            _logger.LogDebug("Short URL {ShortUrlId} expired at {ExpiresAt}",
                id, shortUrl.ExpiresAt);
            return Result<ShortUrl, ShortUrlError>.Failure(ShortUrlError.Expired);
        }

        _logger.LogDebug("Short URL {ShortUrlId} resolved successfully", id);
        return Result<ShortUrl, ShortUrlError>.Success(shortUrl);
    }

    public async Task<Result<ShortUrl, ShortUrlError>> UpdateShortUrlAsync(long shortUrlId, UpdateShortUrlRequest request, int userId, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Updating short URL {ShortUrlId} for user {UserId}", shortUrlId, userId);
        var shortUrl = await GetShortUrlByIdAsync(shortUrlId, userId, cancellationToken);
        if (shortUrl is null)
        {
            _logger.LogDebug("Short URL update rejected because {ShortUrlId} was not found for user {UserId}",
                shortUrlId, userId);
            return Result<ShortUrl, ShortUrlError>.Failure(ShortUrlError.NotFound);
        }

        shortUrl.ExpiresAt = request.ExpiresAt;
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Short URL {ShortUrlId} updated for user {UserId}", shortUrlId, userId);
        return Result<ShortUrl, ShortUrlError>.Success(shortUrl);
    }

    public async Task<Result<ShortUrl, ShortUrlError>> DeleteShortUrlAsync(long shortUrlId, int userId, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Deleting short URL {ShortUrlId} for user {UserId}", shortUrlId, userId);
        var shortUrl = await GetShortUrlByIdAsync(shortUrlId, userId, cancellationToken);
        if (shortUrl is null)
        {
            _logger.LogDebug("Short URL deletion rejected because {ShortUrlId} was not found for user {UserId}",
                shortUrlId, userId);
            return Result<ShortUrl, ShortUrlError>.Failure(ShortUrlError.NotFound);
        }
        
        _dbContext.ShortUrls.Remove(shortUrl);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Short URL {ShortUrlId} deleted for user {UserId}", shortUrlId, userId);
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
        _logger.LogDebug("Saving click event for short URL {ShortUrlId}", clickEvent.ShortUrlId);
        var click = new ShortUrlClick()
        {
            ShortUrlId = clickEvent.ShortUrlId,
            ClickedAt = clickEvent.ClickedAt
        };
        await _dbContext.ShortUrlClicks.AddAsync(click, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogDebug("Click event saved for short URL {ShortUrlId}", clickEvent.ShortUrlId);
    }
}
