using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Surl.Api.Data;
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
            TestConstants.TestUser1, TestConstants.TestPassword, cancellationToken);
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
            TestConstants.TestUser1, TestConstants.TestPassword, cancellationToken);

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
            TestConstants.TestUser1, TestConstants.TestPassword, cancellationToken);
        var stubShortUrl = await ShortUrlTestHelper.CreateStubShortUrlAsync(client,
            accessToken: user1Login.AccessToken, cancellationToken: cancellationToken);
        
        var user2Login = await AuthHelper.RegisterAndLoginAsync(client,
            TestConstants.TestUser2, TestConstants.TestPassword, cancellationToken);

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
            TestConstants.TestUser1, TestConstants.TestPassword, cancellationToken);
        var stubShortUrl = await ShortUrlTestHelper.CreateStubShortUrlAsync(client,
            accessToken: loginResponse.AccessToken, cancellationToken: cancellationToken);
        
        // generate click count
        var expectedClickCount = 10;
        stubShortUrl = stubShortUrl with {ClickCount = expectedClickCount};
        await ShortUrlTestHelper.ClickShortUrlAsync(client, stubShortUrl.ShortUrlCode, stubShortUrl.ClickCount, cancellationToken);
        await ShortUrlTestHelper.WaitUntilAsync(async ct =>
        {
            await using var scope = _factory.Services.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var actual = await db.ShortUrlClicks.LongCountAsync(ct);
            return actual == expectedClickCount;
        });

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
