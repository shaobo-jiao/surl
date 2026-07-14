namespace Surl.Api.ShortUrls;

public class ShortUrl
{
    public long Id { get; set; }
    public string Code { get; set; } = string.Empty; // converted from Id to a short code as the short URL identifier.
    public string OriginalUrl { get; set; } = string.Empty; // original long url to shorten
    public DateTimeOffset ExpiresAt { get; set; } = DateTimeOffset.UtcNow.AddDays(1); // expiry datetime 

    public List<ShortUrlClick> Clicks { get; } = new();
}
