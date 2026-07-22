namespace Surl.Api.ShortUrls;

using Surl.Api.Users;

public class ShortUrl
{
    public long Id { get; set; }
    public string Code { get; set; } = string.Empty; // converted from Id to a short code as the short URL identifier.
    public string OriginalUrl { get; set; } = string.Empty; // original long url to shorten
    public DateTimeOffset ExpiresAt { get; set; } = DateTimeOffset.UtcNow.AddDays(1); // expiry datetime
    public int? UserId { get; set; }
    public User? User { get; set; }

    public List<ShortUrlClick> Clicks { get; } = new();
}
