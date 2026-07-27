using System.Net;
using System.Net.Http.Json;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Surl.Api.IntegrationTests.Common;
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
        var createRequest = new CreateShortUrlRequest(originalUrl);

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
        throw new NotImplementedException();
    }

    [Fact]
    public async Task CreateShortUrl_ValidExpiryDate_ShouldSucceed()
    {
        // Arrange
        var originalUrl = "https://example.com";
        var expiresAt = new DateTimeOffset(2026, 1, 2, 0, 0, 0, TimeSpan.Zero);
        var createRequest = new CreateShortUrlRequest(originalUrl, expiresAt);

        // Act 
        var cancellationToken = TestContext.Current.CancellationToken;
        var httpResponse = await _factory.HttpClient.PostAsJsonAsync("/short-urls", createRequest, cancellationToken);
        var shortUrlResponse = await httpResponse.Content.ReadFromJsonAsync<ShortUrlResponse>(cancellationToken);

        // Assert
        Assert.NotNull(shortUrlResponse);
        Assert.Equal(HttpStatusCode.Created, httpResponse.StatusCode);
        Assert.Equal(originalUrl, shortUrlResponse.OriginalUrl);
        Assert.Equal(expiresAt, shortUrlResponse.ExpiresAt);
    }

    [Fact]
    public async Task CreateShortUrl_InvalidExpiryDate_ShouldReturnBadRequest()
    {
        // Arrange
        var originalUrl = "https://example.com";
        var expiresAt = new DateTimeOffset(2025, 12, 31, 23, 59, 59, TimeSpan.Zero);
        var createRequest = new CreateShortUrlRequest(originalUrl, expiresAt);

        // Act 
        var cancellationToken = TestContext.Current.CancellationToken;
        var httpResponse = await _factory.HttpClient.PostAsJsonAsync("/short-urls", createRequest, cancellationToken);
        var validationResponse = await httpResponse.Content.ReadFromJsonAsync<ValidationProblemDetails>(cancellationToken);

        // Assert
        // status code is BadRequest
        Assert.NotNull(validationResponse);
        Assert.Equal(HttpStatusCode.BadRequest, httpResponse.StatusCode);
        
        // has error on expiresAt
        Assert.Single(validationResponse.Errors);
        var error = validationResponse.Errors.First();
        Assert.Equal("ExpiresAt", error.Key);
        Assert.Equal("The expiry date must be in the future", error.Value.FirstOrDefault());
    }

    [Fact]
    public async Task CreateShortUrl_InvalidOriginalUrl_ShouldReturnBadRequest()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public async Task CreateShortUrl_DisallowedHostOriginalUrl_ShouldReturnBadRequest()
    {
        throw new NotImplementedException();
    }
}
