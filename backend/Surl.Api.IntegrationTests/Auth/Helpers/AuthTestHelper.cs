using Microsoft.Net.Http.Headers;
using Surl.Api.Auth;
using Surl.Api.IntegrationTests.Helpers;

namespace Surl.Api.IntegrationTests.Auth.Helpers;

public static class AuthTestHelper
{
    public const int RefreshTokenExpireInDays = 7; // should match value in JwtOptions from appsettings.

    /// <summary>
    /// Asserts the refresh token cookie is set correctly.
    /// Used to test /login and /refresh endpoints.
    /// </summary>
    public static void AssertRefreshTokenCookieSet(HttpResponseMessage response, DateTimeOffset cookieSetAt)
    {
        var expectedExpires = cookieSetAt.AddDays(RefreshTokenExpireInDays); 
        AssertRefreshTokenCookieSet(response, expectedExpires);
    }

    /// <summary>
    /// Asserts the refresh token cookie is deleted correctly.
    /// Used to test /logout endpoints.
    /// </summary>
    /// <param name="response"></param>
    public static void AssertRefreshTokenCookieDeleted(HttpResponseMessage response)
    {
        AssertRefreshTokenCookieSet(response, DateTimeOffset.UnixEpoch);
    }

    private static void AssertRefreshTokenCookie(HttpResponseMessage response, DateTimeOffset expectedExpiresAt)
    {
        Assert.True(response.Headers.TryGetValues("Set-Cookie", out var setCookies));
        
        // get set-cookie for refresh token
        var cookie = SetCookieHeaderValue.ParseList(setCookies.ToList())
            .SingleOrDefault(x => x.Name.Value == CookieExtensions.RefreshTokenCookieName);

        // assert refresh token set-cookie properties
        Assert.NotNull(cookie);
        Assert.True(cookie.HttpOnly);
        Assert.True(cookie.Secure);
        Assert.Equal(SameSiteMode.Strict, cookie.SameSite);
        Assert.Equal(CookieExtensions.RefreshTokenCookiePath, cookie.Path.Value);

        Assert.Equal(expectedExpiresAt, cookie.Expires);
    }
}
