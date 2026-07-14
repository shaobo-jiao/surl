namespace Surl.Api.ShortUrls;

/// <summary>
/// Represents a short url click event.
/// </summary>
public class ShortUrlClick
{
    public long Id { get; set; }
    
    public long ShortUrlId { get; set; }
    public ShortUrl ShortUrl { get; set; } = default!;

    public DateTimeOffset ClickedAt { get; set; }

    // later to add click source info like referrer, user agents and ip address.
}
