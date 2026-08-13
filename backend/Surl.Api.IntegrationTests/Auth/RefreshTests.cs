using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Surl.Api.Auth;
using Surl.Api.IntegrationTests.Auth.Helpers;
using Surl.Api.IntegrationTests.Helpers;

namespace Surl.Api.IntegrationTests.Auth;

[Collection("SurlApiIntegrationTests")]
public class RefreshTests(CustomWebApplicationFactory factory) : IAsyncLifetime
{
    private readonly CustomWebApplicationFactory _factory = factory;

    public ValueTask InitializeAsync() => ValueTask.CompletedTask;

    public async ValueTask DisposeAsync() => await _factory.ResetStateAsync();

    [Fact]
    public async Task Refresh_ValidToken_ShouldIssueNewTokens()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var (tempClient, cookieContainer) = _factory.CreateClientWithCookieContainer();
        using var client = tempClient; // auto dispose

        // login to get access token in response and refresh token in cookie
        var loginResponse = await AuthHelper.RegisterAndLoginAsync(client,
            TestConstants.TestUser1, TestConstants.TestPassword, cancellationToken);
        var oldRefreshTokenCookie = cookieContainer.GetRefreshTokenCookie();

        // Act
        var httpResponse = await client.PostAsync("/auth/refresh", null, cancellationToken);
        var refreshResponse = await httpResponse.Content.ReadFromJsonAsync<RefreshResponse>(cancellationToken);
        var newRefreshTokenCookie = cookieContainer.GetRefreshTokenCookie();

        // Assert
        Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
        Assert.NotNull(refreshResponse);
        Assert.NotNull(newRefreshTokenCookie);

        Assert.NotEmpty(refreshResponse.AccessToken);
        Assert.NotEmpty(newRefreshTokenCookie.Value);
        // check different refresh token generated. access token is time-dependent so not checked here        
        Assert.NotEqual(oldRefreshTokenCookie.Value, newRefreshTokenCookie.Value);
    }

    [Fact]
    public async Task Refresh_UsedToken_ShouldReturnUnauthorized()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var (tempClient, cookieContainer) = _factory.CreateClientWithCookieContainer();
        using var client = tempClient; // auto dispose

        var loginResponse = await AuthHelper.RegisterAndLoginAsync(client,
            TestConstants.TestUser1, TestConstants.TestPassword, cancellationToken);
        var oldRefreshTokenCookie = cookieContainer.GetRefreshTokenCookie();

        // use the first refresh token
        var firstHttpResponse = await client.PostAsync("/auth/refresh", null, cancellationToken);
        Assert.Equal(HttpStatusCode.OK, firstHttpResponse.StatusCode);

        // Act: set back to the first refresh token
        cookieContainer.SetRefreshTokenCookie(oldRefreshTokenCookie);
        var httpResponse = await client.PostAsync("/auth/refresh", null, cancellationToken);

        // Assert
        await AssertRefreshTokenResponseProblemAsync(AuthError.InvalidRefreshToken, httpResponse, cancellationToken);
    }

    [Fact]
    public async Task Refresh_TokenNotFound_ShouldReturnUnauthorized()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var (tempClient, cookieContainer) = _factory.CreateClientWithCookieContainer();
        using var client = tempClient; // auto dispose

        var loginResponse = await AuthHelper.RegisterAndLoginAsync(client,
            TestConstants.TestUser1, TestConstants.TestPassword, cancellationToken);
        var oldRefreshTokenCookie = cookieContainer.GetRefreshTokenCookie();
        oldRefreshTokenCookie.Value = "NonExistentRefreshToken";

        // Act
        var httpResponse = await client.PostAsync("/auth/refresh", null, cancellationToken);

        // Assert
        await AssertRefreshTokenResponseProblemAsync(AuthError.RefreshTokenNotFound, httpResponse, cancellationToken);
    }

    [Fact]
    public async Task Refresh_RevokedToken_ShouldReturnUnauthorized()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var (tempClient, cookieContainer) = _factory.CreateClientWithCookieContainer();
        using var client = tempClient; // auto dispose

        var loginResponse = await AuthHelper.RegisterAndLoginAsync(client,
            TestConstants.TestUser1, TestConstants.TestPassword, cancellationToken);
        var oldRefreshTokenCookie = cookieContainer.GetRefreshTokenCookie();

        // logout to revoke old refresh token
        await AuthHelper.LogoutAsync(client, cancellationToken);

        // Act: use back the refresh token
        cookieContainer.SetRefreshTokenCookie(oldRefreshTokenCookie);
        var httpResponse = await client.PostAsync("/auth/refresh", null, cancellationToken);

        // Arrange
        await AssertRefreshTokenResponseProblemAsync(AuthError.InvalidRefreshToken, httpResponse, cancellationToken);
    }

    [Fact]
    public async Task Refresh_ExpiredToken_ShouldReturnUnauthorized()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        using var client = _factory.CreateClient();

        var loginResponse = await AuthHelper.RegisterAndLoginAsync(client,
            TestConstants.TestUser1, TestConstants.TestPassword, cancellationToken);

        // Act: advance time to test cookie expiry. 
        // The cookie is still attached in requests because HttpClient uses real time to check cookie expiry
        _factory.TimeProvider.Advance(TimeSpan.FromDays(AuthTestHelper.RefreshTokenExpireInDays + 1));
        var httpResponse = await client.PostAsync("/auth/refresh", null, cancellationToken);

        // Arrange
        await AssertRefreshTokenResponseProblemAsync(AuthError.InvalidRefreshToken, httpResponse, cancellationToken);
    }

    [Fact]
    public async Task Refresh_EmptyToken_ShouldReturnUnauthorized()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        using var client = _factory.CreateClient();

        // Act: directly call refresh endpoint without refresh token cookie. 
        var httpResponse = await client.PostAsync("/auth/refresh", null, cancellationToken);

        // Arrange
        await AssertRefreshTokenResponseProblemAsync(AuthError.EmptyRefreshToken, httpResponse, cancellationToken);
    }

    private static async Task AssertRefreshTokenResponseProblemAsync(
        AuthError expectedError, HttpResponseMessage httpResponse, CancellationToken cancellationToken = default)
    {
        var problemDetails = await httpResponse.Content.ReadFromJsonAsync<ProblemDetails>(cancellationToken);
        
        var expectedProblemDetails = expectedError.ToProblemDetails();
        Assert.NotNull(expectedProblemDetails.Status);

        Assert.Equal((HttpStatusCode)expectedProblemDetails.Status, httpResponse.StatusCode);
        AssertHelper.AssertProblemDetails(expectedProblemDetails, problemDetails);
    }
}
