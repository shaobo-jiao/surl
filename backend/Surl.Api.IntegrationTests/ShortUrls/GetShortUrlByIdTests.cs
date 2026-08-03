using System.Net;
using System.Net.Http.Json;
using Surl.Api.IntegrationTests.Helpers;
using Surl.Api.ShortUrls;

namespace Surl.Api.IntegrationTests.ShortUrls;

[Collection("SurlApiIntegrationTests")]
public class GetShortUrlByIdTests(CustomWebApplicationFactory factory) : IAsyncLifetime
{
    private readonly CustomWebApplicationFactory _factory = factory;

    public ValueTask InitializeAsync() => ValueTask.CompletedTask;

    public async ValueTask DisposeAsync() => await _factory.ResetStateAsync();

    [Fact]
    public async Task GetShortUrlById_AnonymousRequest_ShouldReturnUnauthorized()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;

        // Act
        var response = await _factory.HttpClient.GetAsync("/short-urls/1", cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetShortUrlById_ValidRequest_ShouldReturnShortUrl()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var client = _factory.HttpClient;

        var loginResponse = await AuthHelper.RegisterAndLoginAsync(client,
            Constants.TestUser, Constants.TestPassword, cancellationToken);
        var stubShortUrl = await ShortUrlTestHelper.CreateStubShortUrlAsync(client,
            accessToken: loginResponse.AccessToken, cancellationToken: cancellationToken);

        // Act
        var httpResponse = await client.SendAuthenticatedRequestAsJsonAsync(
            HttpMethod.Get, $"/short-urls/{stubShortUrl.Id}", loginResponse.AccessToken, new { }, cancellationToken);
        var shortUrlResponse = await httpResponse.Content.ReadFromJsonAsync<ShortUrlResponse>(cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
        Assert.NotNull(shortUrlResponse);
        Assert.Equivalent(stubShortUrl, shortUrlResponse);
    }

    [Fact]
    public async Task GetShortUrlById_NonExistentShortUrl_ShouldReturnNotFound()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var client = _factory.HttpClient;
        var loginResponse = await AuthHelper.RegisterAndLoginAsync(client,
            Constants.TestUser, Constants.TestPassword, cancellationToken);

        // Act
        var httpResponse = await client.SendAuthenticatedRequestAsJsonAsync(
            HttpMethod.Get, "/short-urls/12345", loginResponse.AccessToken, new { }, cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, httpResponse.StatusCode);
    }

    [Fact]
    public async Task GetShortUrlById_ShortUrlCreatedByAnotherUser_ShouldReturnNotFound()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var client = _factory.HttpClient;

        var user1Login = await AuthHelper.RegisterAndLoginAsync(client,
            "user1", Constants.TestPassword, cancellationToken);
        var stubShortUrl = await ShortUrlTestHelper.CreateStubShortUrlAsync(client,
            accessToken: user1Login.AccessToken, cancellationToken: cancellationToken);
        
        var user2Login = await AuthHelper.RegisterAndLoginAsync(client,
            "user2", Constants.TestPassword, cancellationToken);

        // Act
        var httpResponse = await client.SendAuthenticatedRequestAsJsonAsync(
            HttpMethod.Get, $"/short-urls/{stubShortUrl.Id}", user2Login.AccessToken, new { }, cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, httpResponse.StatusCode);
    }
}
