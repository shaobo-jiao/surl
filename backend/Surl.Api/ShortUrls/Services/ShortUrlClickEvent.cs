namespace Surl.Api.ShortUrls;

/// <summary>
/// Represents a short url click event DTO.
/// </summary>
public record ShortUrlClickEvent(long ShortUrlId, string ShortUrlCode, DateTimeOffset ClickedAt);
