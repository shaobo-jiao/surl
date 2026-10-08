namespace Surl.Api.ShortUrls;

// ExpiresAt null means the short URL never expires
public record UpdateShortUrlRequest(DateTimeOffset? ExpiresAt = null);
