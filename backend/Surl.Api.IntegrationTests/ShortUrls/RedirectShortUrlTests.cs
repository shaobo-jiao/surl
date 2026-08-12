using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Surl.Api.IntegrationTests.Helpers;
using Surl.Api.ShortUrls;

namespace Surl.Api.IntegrationTests.ShortUrls;

[Collection("SurlApiIntegrationTests")]
public class RedirectShortUrlTests(CustomWebApplicationFactory factory) : IAsyncLifetime
{
    private readonly CustomWebApplicationFactory _factory = factory;
    public ValueTask InitializeAsync() => ValueTask.CompletedTask;
    public async ValueTask DisposeAsync() => await _factory.ResetStateAsync();

    // test 1: invalid code => not found
    // test 2: non-existent short url => not found
    // test 3: expired short url => done, expired 
    // test 4: valid request => get expected original url & click count++

    [Fact]
    public async Task RedirectShortUrl_InvalidCode_ShouldReturnNotFound()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        using var client = _factory.CreateClient();

        // Act
        var httpResponse = await client.GetAsync("/redirection/INVALID_CODE", cancellationToken);
        var problemDetails = await httpResponse.Content.ReadFromJsonAsync<ProblemDetails>(cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, httpResponse.StatusCode);
        var expected = ShortUrlError.InvalidCode.ToProblemDetails();
        AssertHelper.AssertProblemDetails(expected, problemDetails);
    }

    [Fact]
    public async Task RedirectShortUrl_NonExistent_ShouldReturnNotFound()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        using var client = _factory.CreateClient();

        // Act
        var httpResponse = await client.GetAsync("/redirection/12345", cancellationToken);
        var problemDetails = await httpResponse.Content.ReadFromJsonAsync<ProblemDetails>(cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, httpResponse.StatusCode);
        var expected = ShortUrlError.NotFound.ToProblemDetails();
        AssertHelper.AssertProblemDetails(expected, problemDetails);
    }

    [Fact]
    public async Task RedirectShortUrl_Expired_ShouldReturnExpiryError()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        using var client = _factory.CreateClient();

        var shortUrl = await ShortUrlTestHelper.CreateStubShortUrlAsync(client,
            originalUrl: "https://www.example.com",
            expiresAt: Constants.TestStartTime.AddDays(1),
            cancellationToken: cancellationToken);

        // Act
        _factory.TimeProvider.Advance(TimeSpan.FromDays(1.1));
        var httpResponse = await client.GetAsync($"/redirection/{shortUrl.ShortUrlCode}", cancellationToken);
        var problemDetails = await httpResponse.Content.ReadFromJsonAsync<ProblemDetails>(cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Gone, httpResponse.StatusCode);
        var expected = ShortUrlError.Expired.ToProblemDetails();
        AssertHelper.AssertProblemDetails(expected, problemDetails);
    }

    [Fact]
    public async Task RedirectShortUrl_ValidRequest_ShouldReturnOriginalUrlWithCountIncrement()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        using var client = _factory.CreateClient();

        var userLogin = await AuthHelper.RegisterAndLoginAsync(
            client, Constants.TestUser, Constants.TestPassword, cancellationToken);

        var originalUrl = "https://www.example.com";
        var shortUrl = await ShortUrlTestHelper.CreateStubShortUrlAsync(client,
            originalUrl: originalUrl,
            expiresAt: Constants.TestStartTime.AddDays(1),
            accessToken: userLogin.AccessToken,
            cancellationToken: cancellationToken);

        // Act
        var httpResponse = await client.GetAsync($"/redirection/{shortUrl.ShortUrlCode}", cancellationToken);
        var actualOriginalUrl = await httpResponse.Content.ReadFromJsonAsync<string>(cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
        Assert.Equal(originalUrl, actualOriginalUrl);

        // validate click count
        var queryShortUrlResponse = await client.SendAuthenticatedRequestAsJsonAsync(
            HttpMethod.Get, $"/short-urls/{shortUrl.Id}", userLogin.AccessToken, cancellationToken);
        var shortUrlResult = await queryShortUrlResponse.Content.ReadFromJsonAsync<ShortUrlResponse>(cancellationToken);
        Assert.NotNull(shortUrlResult);
        Assert.Equal(1, shortUrlResult.ClickCount);
    }
}
