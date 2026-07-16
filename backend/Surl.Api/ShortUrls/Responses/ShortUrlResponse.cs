namespace Surl.Api.ShortUrls;

public record ShortUrlResponse(long Id, string ShortUrlCode, string OriginalUrl, DateTimeOffset ExpiresAt);

public static class ShortUrlResponseMapper
{
    public static ShortUrlResponse ToResponse(this ShortUrl shortUrl)
    {
        return new ShortUrlResponse(shortUrl.Id, shortUrl.Code, shortUrl.OriginalUrl, shortUrl.ExpiresAt);
    }
}