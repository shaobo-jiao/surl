namespace Surl.Api.ShortUrls;

public record CreateShortUrlRequest(string OriginalUrl, DateTimeOffset? ExpiresAt = null);
