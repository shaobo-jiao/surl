namespace Surl.Api.ShortUrls;

public record QueryShortUrlParams(
    DateTimeOffset? ExpiresFrom, 
    DateTimeOffset? ExpiresTo, 
    string? OrderBy, 
    bool? IsDescending,
    int? Page, 
    int? PageSize);

public static class QueryShortUrlAllowedOrderByProperties
{
    public const string ExpiresAt = "ExpiresAt";
    public const string ClickCount = "ClickCount";
    
    public static HashSet<string> All() => [ExpiresAt, ClickCount];
}