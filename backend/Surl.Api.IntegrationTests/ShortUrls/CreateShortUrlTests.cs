using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Surl.Api.IntegrationTests.Helpers;
using Surl.Api.ShortUrls;

namespace Surl.Api.IntegrationTests.ShortUrls;

[Collection("SurlApiIntegrationTests")]
public class CreateShortUrlTests(CustomWebApplicationFactory factory) : IAsyncLifetime
{
    private readonly CustomWebApplicationFactory _factory = factory;
    public ValueTask InitializeAsync() => ValueTask.CompletedTask;

    public async ValueTask DisposeAsync() => await _factory.ResetStateAsync();

    [Fact]
    public async Task CreateShortUrl_ValidAnonymousRequest_ShouldSucceed()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        using var client = _factory.CreateClient();

        var originalUrl = "https://example.com";
        var createRequest = new CreateShortUrlRequest(originalUrl,
            ExpiresAt: TestConstants.TestStartTime.AddMinutes(1));

        // Act 
        var httpResponse = await client.PostAsJsonAsync("/short-urls/anonymous", createRequest, cancellationToken);
        var shortUrlResponse = await httpResponse.Content.ReadFromJsonAsync<ShortUrlResponse>(cancellationToken);

        // Assert
        Assert.NotNull(shortUrlResponse);
        Assert.Equal(HttpStatusCode.Created, httpResponse.StatusCode);
        Assert.Equal(originalUrl, shortUrlResponse.OriginalUrl);
        Assert.Null(shortUrlResponse.UserId);
    }

    [Fact]
    public async Task CreateShortUrl_ValidAuthenticatedRequest_ShouldSucceed()
    {
        // Arrange: creates a user and creates short url for the user
        var cancellationToken = TestContext.Current.CancellationToken;
        using var httpClient = _factory.CreateClient();

        var user = await AuthHelper.RegisterAsync(httpClient, TestConstants.TestUser1, TestConstants.TestPassword, cancellationToken);
        var authTokens = await AuthHelper.LoginAsync(httpClient, TestConstants.TestUser1, TestConstants.TestPassword, cancellationToken);

        var originalUrl = "https://example.com";
        var createShortUrlRequest = new CreateShortUrlRequest(originalUrl);

        // Act: create a short url for given user;
        var httpResponse = await httpClient.SendAuthenticatedRequestAsJsonAsync(
            HttpMethod.Post, "/short-urls", authTokens.AccessToken, createShortUrlRequest, cancellationToken);
        var shortUrlResponse = await httpResponse.Content.ReadFromJsonAsync<ShortUrlResponse>(cancellationToken);

        // Assert
        Assert.NotNull(shortUrlResponse);
        Assert.Equal(HttpStatusCode.Created, httpResponse.StatusCode);
        Assert.Equal(originalUrl, shortUrlResponse.OriginalUrl);
        Assert.Equal(user.Id, shortUrlResponse.UserId);
    }

    [Fact]
    public async Task CreateShortUrl_AuthenticatedEndpointWithoutToken_ShouldReturnUnauthorized()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        using var client = _factory.CreateClient();

        var createRequest = new CreateShortUrlRequest("https://example.com");

        // Act: hitting the authenticated endpoint anonymously should be rejected, not treated as anonymous creation
        var httpResponse = await client.PostAsJsonAsync("/short-urls", createRequest, cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, httpResponse.StatusCode);
    }

    /// <summary>
    /// There's only one method here to test the request validator is working. 
    /// Detailed testing on the request validator are in unit tests. 
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task CreateShortUrl_InvalidRequestFields_ShouldReturnValidationFailure()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        using var client = _factory.CreateClient();

        var createRequest = new CreateShortUrlRequest(
            OriginalUrl: "INVALID_URL",
            ExpiresAt: TestConstants.TestStartTime.AddMinutes(-1));

        // Act
        var httpResponse = await client.PostAsJsonAsync("/short-urls/anonymous", createRequest, cancellationToken);
        var validationDetails = await httpResponse.Content.ReadFromJsonAsync<ValidationProblemDetails>(cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, httpResponse.StatusCode);
        Assert.NotNull(validationDetails);
        Assert.Equivalent(new HashSet<string> { "originalUrl", "expiresAt" },
            validationDetails.Errors.Keys);
    }
}
