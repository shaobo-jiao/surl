using System.Text.Json.Serialization;

namespace Surl.Api.ShortUrls;

public record ShortUrlResponse(
    [property: JsonNumberHandling(JsonNumberHandling.WriteAsString)]
    long Id,
    string ShortUrlCode,
    string OriginalUrl,
    DateTimeOffset? ExpiresAt,
    long ClickCount,
    int? UserId);

public static class ShortUrlResponseMapper
{
    public static ShortUrlResponse ToResponse(this ShortUrl shortUrl)
    {
        return new ShortUrlResponse(shortUrl.Id,
            shortUrl.Code,
            shortUrl.OriginalUrl,
            shortUrl.ExpiresAt,
            shortUrl.Clicks.Count, // ensure Clicks are loaded
            shortUrl.UserId);
    }
}
