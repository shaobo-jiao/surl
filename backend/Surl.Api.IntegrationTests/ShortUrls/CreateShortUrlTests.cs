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
        var originalUrl = "https://example.com";
        var createRequest = new CreateShortUrlRequest(originalUrl,
            ExpiresAt: Constants.TestStartTime.AddMinutes(1));

        // Act 
        var cancellationToken = TestContext.Current.CancellationToken;
        var httpResponse = await _factory.HttpClient.PostAsJsonAsync("/short-urls", createRequest, cancellationToken);
        var shortUrlResponse = await httpResponse.Content.ReadFromJsonAsync<ShortUrlResponse>(cancellationToken);

        // Assert
        Assert.NotNull(shortUrlResponse);
        Assert.Equal(HttpStatusCode.Created, httpResponse.StatusCode);
        Assert.Equal(originalUrl, shortUrlResponse.OriginalUrl);
    }

    [Fact]
    public async Task CreateShortUrl_ValidAuthenticatedRequest_ShouldSucceed()
    {
        // Arrange: creates a user and creates short url for the user
        var httpClient = _factory.HttpClient;
        var cancellationToken = TestContext.Current.CancellationToken;

        var user = await AuthHelper.RegisterAsync(httpClient, "user1", cancellationToken);
        var authTokens = await AuthHelper.LoginAsync(httpClient, "user1", cancellationToken);

        var originalUrl = "https://example.com";
        var createShortUrlRequest = new CreateShortUrlRequest(originalUrl);
        var httpRequestMessage = AuthHelper.CreateAuthenticatedHttpRequestMessageAsJson(
            HttpMethod.Post, "/short-urls", authTokens.AccessToken, createShortUrlRequest);

        // Act: create a short url for given user;
        var httpResponse = await httpClient.SendAsync(httpRequestMessage, cancellationToken);
        var shortUrlResponse = await httpResponse.Content.ReadFromJsonAsync<ShortUrlResponse>(cancellationToken);

        // Assert
        Assert.NotNull(shortUrlResponse);
        Assert.Equal(HttpStatusCode.Created, httpResponse.StatusCode);
        Assert.Equal(originalUrl, shortUrlResponse.OriginalUrl);
        Assert.Equal(user.Id, shortUrlResponse.UserId);
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
        var client = _factory.HttpClient;

        var createRequest = new CreateShortUrlRequest(
            OriginalUrl: "INVALID_URL",
            ExpiresAt: Constants.TestStartTime.AddMinutes(-1));

        // Act
        var httpResponse = await _factory.HttpClient.PostAsJsonAsync("/short-urls", createRequest, cancellationToken);
        var validationDetails = await httpResponse.Content.ReadFromJsonAsync<ValidationProblemDetails>(cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, httpResponse.StatusCode);
        Assert.NotNull(validationDetails);
        Assert.Equivalent(new HashSet<string> { "OriginalUrl", "ExpiresAt" },
            validationDetails.Errors.Keys);
    }
}
