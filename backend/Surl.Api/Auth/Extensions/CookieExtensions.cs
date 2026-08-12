namespace Surl.Api.Auth;

public static class CookieExtensions
{
    public const string RefreshTokenCookieName = "refreshToken";
    public const string RefreshTokenCookiePath = "/auth";

    public static string? GetRefreshToken(this IRequestCookieCollection cookies)
    {
        if (cookies.TryGetValue(RefreshTokenCookieName, out string? refreshToken))
            return refreshToken;
        return null;
    }

    public static void SetRefreshToken(this IResponseCookies cookies, RefreshToken refreshToken)
    {
        cookies.Append(RefreshTokenCookieName, refreshToken.Token, new CookieOptions()
        {
            Path = RefreshTokenCookiePath,
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = refreshToken.ExpiresAt,
        });
    }
    public static void DeleteRefreshToken(this IResponseCookies cookies)
    {
        cookies.Delete(RefreshTokenCookieName, new CookieOptions()
        {
            Path = RefreshTokenCookiePath,
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
        });
    }
}
