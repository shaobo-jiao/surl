namespace UrlShortener.Api;

public record ShortUrlResponse(long Id, string ShortUrl, string OriginalUrl, DateTimeOffset ExpiresAt);

public static class ShortUrlResponseMapper
{
    public static ShortUrlResponse ToResponse(this ShortUrl shortUrl, ApiSettings apiSettings)
    {
        var builder = new UriBuilder(apiSettings.ApiBaseUrl)
        {
            Path = shortUrl.Code
        };
        return new ShortUrlResponse(shortUrl.Id, builder.Uri.AbsoluteUri, shortUrl.OriginalUrl, 
            shortUrl.ExpiresAt);
    }
}