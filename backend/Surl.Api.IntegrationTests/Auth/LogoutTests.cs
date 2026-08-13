using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Surl.Api.Auth;
using Surl.Api.Data;
using Surl.Api.IntegrationTests.Auth.Helpers;
using Surl.Api.IntegrationTests.Helpers;

namespace Surl.Api.IntegrationTests.Auth;

[Collection("SurlApiIntegrationTests")]
public class LogoutTests(CustomWebApplicationFactory factory) : IAsyncLifetime
{
    private readonly CustomWebApplicationFactory _factory = factory;

    public ValueTask InitializeAsync() => ValueTask.CompletedTask;

    public async ValueTask DisposeAsync() => await _factory.ResetStateAsync();

    [Fact]
    public async Task Logout_ValidToken_ShouldSucceedAndDeleteCookie()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var (tempClient, cookieContainer) = _factory.CreateClientWithCookieContainer();
        using var client = tempClient; // auto dispose

        await AuthHelper.RegisterAndLoginAsync(client,
            TestConstants.TestUser1, TestConstants.TestPassword, cancellationToken);
        var refreshTokenCookie = cookieContainer.GetRefreshTokenCookie();

        // Act
        var httpResponse = await client.PostAsync("/auth/logout", null, cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
        AuthTestHelper.AssertRefreshTokenCookieDeleted(httpResponse);
    }

    [Fact]
    public async Task Logout_EmptyToken_ShouldReturnUnauthorized()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        using var client = _factory.CreateClient();

        // Act: directly call logout without refresh token cookie
        var httpResponse = await client.PostAsync("/auth/logout", null, cancellationToken);

        // Assert
        await AssertLogoutResponseProblemAsync(AuthError.EmptyRefreshToken, httpResponse, cancellationToken);
    }

    [Fact]
    public async Task Logout_TokenNotFound_ShouldReturnUnauthorized()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var (tempClient, cookieContainer) = _factory.CreateClientWithCookieContainer();
        using var client = tempClient; // auto dispose

        await AuthHelper.RegisterAndLoginAsync(client,
            TestConstants.TestUser1, TestConstants.TestPassword, cancellationToken);
        var refreshTokenCookie = cookieContainer.GetRefreshTokenCookie();
        refreshTokenCookie.Value = "NonExistentRefreshToken";

        // Act
        var httpResponse = await client.PostAsync("/auth/logout", null, cancellationToken);

        // Assert
        await AssertLogoutResponseProblemAsync(AuthError.RefreshTokenNotFound, httpResponse, cancellationToken);
    }

    [Fact]
    public async Task Logout_UsedToken_ShouldReturnUnauthorized()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var (tempClient, cookieContainer) = _factory.CreateClientWithCookieContainer();
        using var client = tempClient; // auto dispose

        await AuthHelper.RegisterAndLoginAsync(client,
            TestConstants.TestUser1, TestConstants.TestPassword, cancellationToken);
        var oldRefreshTokenCookie = cookieContainer.GetRefreshTokenCookie();

        // use (and invalidate) the first refresh token via /auth/refresh
        var firstHttpResponse = await client.PostAsync("/auth/refresh", null, cancellationToken);
        Assert.Equal(HttpStatusCode.OK, firstHttpResponse.StatusCode);

        // Act: set back the now-used refresh token
        cookieContainer.SetRefreshTokenCookie(oldRefreshTokenCookie);
        var httpResponse = await client.PostAsync("/auth/logout", null, cancellationToken);

        // Assert
        await AssertLogoutResponseProblemAsync(AuthError.InvalidRefreshToken, httpResponse, cancellationToken);
    }

    [Fact]
    public async Task Logout_RevokedToken_ShouldReturnUnauthorized()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var (tempClient, cookieContainer) = _factory.CreateClientWithCookieContainer();
        using var client = tempClient; // auto dispose

        await AuthHelper.RegisterAndLoginAsync(client,
            TestConstants.TestUser1, TestConstants.TestPassword, cancellationToken);
        var oldRefreshTokenCookie = cookieContainer.GetRefreshTokenCookie();

        // revoke the refresh token via a first logout
        await AuthHelper.LogoutAsync(client, cancellationToken);

        // Act: set back the revoked refresh token
        cookieContainer.SetRefreshTokenCookie(oldRefreshTokenCookie);
        var httpResponse = await client.PostAsync("/auth/logout", null, cancellationToken);

        // Assert
        await AssertLogoutResponseProblemAsync(AuthError.InvalidRefreshToken, httpResponse, cancellationToken);
    }

    [Fact]
    public async Task Logout_ExpiredToken_ShouldReturnUnauthorized()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        using var client = _factory.CreateClient();

        await AuthHelper.RegisterAndLoginAsync(client,
            TestConstants.TestUser1, TestConstants.TestPassword, cancellationToken);

        // Act: advance time to test token expiry.
        // The cookie is still attached in requests because HttpClient uses real time to check cookie expiry
        _factory.TimeProvider.Advance(TimeSpan.FromDays(AuthTestHelper.RefreshTokenExpireInDays + 1));
        var httpResponse = await client.PostAsync("/auth/logout", null, cancellationToken);

        // Assert
        await AssertLogoutResponseProblemAsync(AuthError.InvalidRefreshToken, httpResponse, cancellationToken);
    }

    private static async Task AssertLogoutResponseProblemAsync(
        AuthError expectedError, HttpResponseMessage httpResponse, CancellationToken cancellationToken = default)
    {
        var problemDetails = await httpResponse.Content.ReadFromJsonAsync<ProblemDetails>(cancellationToken);

        var expectedProblemDetails = expectedError.ToProblemDetails();
        Assert.NotNull(expectedProblemDetails.Status);

        Assert.Equal((HttpStatusCode)expectedProblemDetails.Status, httpResponse.StatusCode);
        AssertHelper.AssertProblemDetails(expectedProblemDetails, problemDetails);
    }
}
