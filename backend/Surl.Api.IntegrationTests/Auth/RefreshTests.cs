using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Surl.Api.Auth;
using Surl.Api.Data;
using Surl.Api.IntegrationTests.Helpers;

namespace Surl.Api.IntegrationTests.Auth;

[Collection("SurlApiIntegrationTests")]
public class RefreshTests(CustomWebApplicationFactory factory) : IAsyncLifetime
{
    #region Life Cycle
    private readonly CustomWebApplicationFactory _factory = factory;

    public ValueTask InitializeAsync() => ValueTask.CompletedTask;

    public async ValueTask DisposeAsync() => await _factory.ResetStateAsync();
    #endregion

    [Fact]
    public async Task Refresh_ValidToken_ShouldIssueNewTokens()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        using var client = _factory.CreateClient();

        var loginResponse = await AuthHelper.RegisterAndLoginAsync(client, 
            Constants.TestUser1, Constants.TestPassword, cancellationToken);
        var refreshRequest = new RefreshRequest(loginResponse.RefreshToken);

        // Act
        var httpResponse = await client.PostAsJsonAsync("/auth/refresh", refreshRequest, cancellationToken);
        var refreshResponse = await httpResponse.Content.ReadFromJsonAsync<RefreshResponse>(cancellationToken);
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
        Assert.NotNull(refreshResponse);

        Assert.NotEmpty(refreshResponse.AccessToken);
        Assert.NotEmpty(refreshResponse.RefreshToken);

        // access token are not checked here because it's time dependent. same token at the same time.        
        Assert.NotEqual(loginResponse.RefreshToken, refreshResponse.RefreshToken);
    }

    [Fact]
    public async Task Refresh_UsedToken_ShouldReturnUnauthorized()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        using var client = _factory.CreateClient();

        var loginResponse = await AuthHelper.RegisterAndLoginAsync(client, 
            Constants.TestUser1, Constants.TestPassword, cancellationToken);
        var refreshRequest = new RefreshRequest(loginResponse.RefreshToken);

        var firstResponse = await client.PostAsJsonAsync("/auth/refresh", refreshRequest, cancellationToken);
        Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode);

        // Act
        var httpResponse = await client.PostAsJsonAsync("/auth/refresh", refreshRequest, cancellationToken);
        var problemDetails = await httpResponse.Content.ReadFromJsonAsync<ProblemDetails>(cancellationToken);

        // Assert
        AssertInvalidRefreshToken(httpResponse, problemDetails);
    }

    [Fact]
    public async Task Refresh_TokenNotFound_ShouldReturnUnauthorized()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        using var client = _factory.CreateClient();
        var refreshRequest = new RefreshRequest("refresh-token-that-does-not-exist");

        // Act
        var httpResponse = await client.PostAsJsonAsync("/auth/refresh", refreshRequest, cancellationToken);
        var problemDetails = await httpResponse.Content.ReadFromJsonAsync<ProblemDetails>(cancellationToken);

        // Assert
        AssertInvalidRefreshToken(httpResponse, problemDetails);
    }

    [Fact]
    public async Task Refresh_RevokedToken_ShouldReturnUnauthorized()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        using var client = _factory.CreateClient();
        var loginResponse = await AuthHelper.RegisterAndLoginAsync(client, 
            Constants.TestUser1, Constants.TestPassword, cancellationToken);

        // revoke the token first
        await using (var scope = _factory.Services.CreateAsyncScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var refreshToken = await dbContext.RefreshTokens.SingleAsync(
                token => token.Token == loginResponse.RefreshToken, cancellationToken);
            refreshToken.IsRevoked = true;
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        var refreshRequest = new RefreshRequest(loginResponse.RefreshToken);

        // Act
        var httpResponse = await client.PostAsJsonAsync("/auth/refresh", refreshRequest, cancellationToken);
        var problemDetails = await httpResponse.Content.ReadFromJsonAsync<ProblemDetails>(cancellationToken);

        // Assert
        AssertInvalidRefreshToken(httpResponse, problemDetails);
    }

    private static void AssertInvalidRefreshToken(
        HttpResponseMessage httpResponse, ProblemDetails? problemDetails)
    {
        Assert.Equal(HttpStatusCode.Unauthorized, httpResponse.StatusCode);
        var expected = AuthError.InvalidRefreshToken.ToProblemDetails();
        AssertHelper.AssertProblemDetails(expected, problemDetails);
    }
}
