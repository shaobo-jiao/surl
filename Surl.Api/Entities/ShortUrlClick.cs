namespace Surl.Api;

/// <summary>
/// Represents a short url click event.
/// </summary>
public class ShortUrlClick
{
    public long Id { get; set; }
    
    public long ShortUrlId { get; set; }
    public ShortUrl ShortUrl { get; set; } = default!;

    public DateTimeOffset ClickedAt { get; set; }

    // later to add click source info like referer, user agents and ip address.
}
