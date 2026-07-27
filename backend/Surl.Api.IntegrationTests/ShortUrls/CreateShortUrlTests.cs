using System.Net;
using System.Net.Http.Json;
using Surl.Api.IntegrationTests.Common;
using Surl.Api.ShortUrls;

namespace Surl.Api.IntegrationTests.ShortUrls;

[Collection("SurlApiIntegrationTests")]
public class CreateShortUrlTests(CustomWebApplicationFactory factory) : IAsyncLifetime
{
    private readonly CustomWebApplicationFactory _factory = factory;
    public ValueTask InitializeAsync() => ValueTask.CompletedTask;

    public async ValueTask DisposeAsync() => await _factory.ResetDatabaseAsync();

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
}
