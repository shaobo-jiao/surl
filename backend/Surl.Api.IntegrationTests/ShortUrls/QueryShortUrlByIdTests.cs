using System.Net;
using System.Net.Http.Json;
using Surl.Api.IntegrationTests.Helpers;
using Surl.Api.ShortUrls;

namespace Surl.Api.IntegrationTests.ShortUrls;

[Collection("SurlApiIntegrationTests")]
public class QueryShortUrlByIdTests(CustomWebApplicationFactory factory) : IAsyncLifetime
{
    private readonly CustomWebApplicationFactory _factory = factory;

    public ValueTask InitializeAsync() => ValueTask.CompletedTask;

    public async ValueTask DisposeAsync() => await _factory.ResetStateAsync();

    [Fact]
    public async Task QueryShortUrlById_AnonymousRequest_ShouldReturnUnauthorized()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;

        // Act
        using var client = _factory.CreateClient();
        var response = await client.GetAsync("/short-urls/1", cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task QueryShortUrlById_ValidRequest_ShouldReturnShortUrl()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        using var client = _factory.CreateClient();

        var loginResponse = await AuthHelper.RegisterAndLoginAsync(client,
            Constants.TestUser, Constants.TestPassword, cancellationToken);
        var stubShortUrl = await ShortUrlTestHelper.CreateStubShortUrlAsync(client,
            accessToken: loginResponse.AccessToken, cancellationToken: cancellationToken);

        // Act
        var httpResponse = await client.SendAuthenticatedRequestAsJsonAsync(
            HttpMethod.Get, $"/short-urls/{stubShortUrl.Id}", loginResponse.AccessToken, cancellationToken);
        var shortUrlResponse = await httpResponse.Content.ReadFromJsonAsync<ShortUrlResponse>(cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
        Assert.NotNull(shortUrlResponse);
        Assert.Equivalent(stubShortUrl, shortUrlResponse);
    }

    [Fact]
    public async Task QueryShortUrlById_NonExistentShortUrl_ShouldReturnNotFound()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        using var client = _factory.CreateClient();
        var loginResponse = await AuthHelper.RegisterAndLoginAsync(client,
            Constants.TestUser, Constants.TestPassword, cancellationToken);

        // Act
        var httpResponse = await client.SendAuthenticatedRequestAsJsonAsync(
            HttpMethod.Get, "/short-urls/12345", loginResponse.AccessToken, cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, httpResponse.StatusCode);
    }

    [Fact]
    public async Task QueryShortUrlById_ShortUrlCreatedByAnotherUser_ShouldReturnNotFound()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        using var client = _factory.CreateClient();

        var user1Login = await AuthHelper.RegisterAndLoginAsync(client,
            "user1", Constants.TestPassword, cancellationToken);
        var stubShortUrl = await ShortUrlTestHelper.CreateStubShortUrlAsync(client,
            accessToken: user1Login.AccessToken, cancellationToken: cancellationToken);
        
        var user2Login = await AuthHelper.RegisterAndLoginAsync(client,
            "user2", Constants.TestPassword, cancellationToken);

        // Act
        var httpResponse = await client.SendAuthenticatedRequestAsJsonAsync(
            HttpMethod.Get, $"/short-urls/{stubShortUrl.Id}", user2Login.AccessToken, cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, httpResponse.StatusCode);
    }
    
    [Fact]
    public async Task QueryShortUrlById_ValidRequestWithClickCount_ShouldReturnShortUrlWithClickCount()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        using var client = _factory.CreateClient();

        var loginResponse = await AuthHelper.RegisterAndLoginAsync(client,
            Constants.TestUser, Constants.TestPassword, cancellationToken);
        var stubShortUrl = await ShortUrlTestHelper.CreateStubShortUrlAsync(client,
            accessToken: loginResponse.AccessToken, cancellationToken: cancellationToken);
        // generate click count
        stubShortUrl = stubShortUrl with {ClickCount = 10};
        await ShortUrlTestHelper.ClickShortUrlAsync(client, stubShortUrl.ShortUrlCode, stubShortUrl.ClickCount, cancellationToken);
        await Task.Delay(100, cancellationToken); // delay a bit for the click background service  
        // Act
        var httpResponse = await client.SendAuthenticatedRequestAsJsonAsync(
            HttpMethod.Get, $"/short-urls/{stubShortUrl.Id}", loginResponse.AccessToken, cancellationToken);
        var shortUrlResponse = await httpResponse.Content.ReadFromJsonAsync<ShortUrlResponse>(cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
        Assert.NotNull(shortUrlResponse);
        Assert.Equivalent(stubShortUrl, shortUrlResponse);
    }
}
