using Surl.Api.Common;
using Surl.Api.Data;

namespace Surl.Api.ShortUrls;

public class ShortUrlService(AppDbContext dbContext, IdGenerator<ShortUrl> idGenerator)
{
    private AppDbContext _dbContext = dbContext;
    private IdGenerator<ShortUrl> _idGenerator = idGenerator;

    public async Task<ShortUrl> CreateShortUrlAsync(CreateShortUrlRequest request, CancellationToken cancellationToken)
    {
        // generate ID and encode code
        var newId = _idGenerator.NewId();
        var shortUrl = new ShortUrl
        {
            Id = newId,
            Code = Base62Encoding.Encode(newId),
            OriginalUrl = request.OriginalUrl
        };
        await _dbContext.ShortUrls.AddAsync(shortUrl);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return shortUrl;
    }

    public async Task<ShortUrl> GetShortUrlByCodeAsync(string code, CancellationToken cancellationToken)
    {
        if (!Base62Encoding.TryDecode(code, out long id))
            throw new ShortUrlInvalidCodeException();

        var shortUrl = await _dbContext.ShortUrls.FindAsync(id, cancellationToken) 
            ?? throw new ShortUrlNotFoundException();
        if (shortUrl.ExpiresAt < DateTimeOffset.UtcNow) 
            throw new ShortUrlExpiredException();
        return shortUrl;
    }

    /// <summary>
    /// Saves a Short URL click event into database.
    /// </summary>
    /// <param name="clickEvent"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task SaveClickEventAsync(ShortUrlClickEvent clickEvent, CancellationToken cancellationToken)
    {
        var click = new ShortUrlClick()
        {
            ShortUrlId = clickEvent.ShortUrlId,
            ClickedAt = clickEvent.ClickedAt
        };
        await _dbContext.ShortUrlClicks.AddAsync(click, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
