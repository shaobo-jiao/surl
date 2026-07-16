namespace Surl.Api;

public class JwtOptions
{
    public required string SecretKey { get; init; }
    public required string Issuer { get; init; }
    public required string Audience { get; init; }
    public required int AccessTokenExpireInMinutes { get; init; } 
    public required int RefreshTokenExpireInDays { get; init; }
}
