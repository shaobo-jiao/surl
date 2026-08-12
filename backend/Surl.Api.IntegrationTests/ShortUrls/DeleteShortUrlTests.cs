using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Surl.Api.IntegrationTests.Helpers;
using Surl.Api.ShortUrls;

namespace Surl.Api.IntegrationTests.ShortUrls;

[Collection("SurlApiIntegrationTests")]
public class DeleteShortUrlTests(CustomWebApplicationFactory factory) : IAsyncLifetime
{
    private readonly CustomWebApplicationFactory _factory = factory;
    public ValueTask InitializeAsync() => ValueTask.CompletedTask;

    public async ValueTask DisposeAsync() => await _factory.ResetStateAsync();

    [Fact]
    public async Task DeleteShortUrl_AnonymousRequest_ShouldReturnUnauthorized()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        using var client = _factory.CreateClient();

        // Act
        var response = await client.DeleteAsync("/short-urls/1", cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task DeleteShortUrl_ValidRequest_ShouldDeleteSuccessfully()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        using var client = _factory.CreateClient();

        // create a test url
        var loginResponse = await AuthHelper.RegisterAndLoginAsync(client,
            Constants.TestUser, Constants.TestPassword, cancellationToken);
        var accessToken = loginResponse.AccessToken;
        var stubShortUrl = await ShortUrlTestHelper.CreateStubShortUrlAsync(client,
            accessToken: accessToken, cancellationToken: cancellationToken);

        // Act
        var deleteResponse = await client.SendAuthenticatedRequestAsJsonAsync(
            HttpMethod.Delete, $"/short-urls/{stubShortUrl.Id}", accessToken, cancellationToken);
        var getResponse = await client.SendAuthenticatedRequestAsJsonAsync(
            HttpMethod.Get, $"/short-urls/{stubShortUrl.Id}", accessToken, cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task DeleteShortUrl_NonExistentShortUrl_ShouldReturnNotFound()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        using var client = _factory.CreateClient();
        var loginResponse = await AuthHelper.RegisterAndLoginAsync(client,
            Constants.TestUser, Constants.TestPassword, cancellationToken);

        // Act
        var httpResponse = await client.SendAuthenticatedRequestAsJsonAsync(
            HttpMethod.Delete, "/short-urls/12345", loginResponse.AccessToken, cancellationToken);
        var problemDetails = await httpResponse.Content.ReadFromJsonAsync<ProblemDetails>(cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, httpResponse.StatusCode);
        var expected = ShortUrlError.NotFound.ToProblemDetails();
        AssertHelper.AssertProblemDetails(expected, problemDetails);
    }

    [Fact]
    public async Task DeleteShortUrl_DeletingUrlCreatedByOthers_ShouldReturnNotFound()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        using var client = _factory.CreateClient();

        // create a test url of user1
        var user1Login = await AuthHelper.RegisterAndLoginAsync(client,
            "user1", Constants.TestPassword, cancellationToken);
        var stubShortUrl = await ShortUrlTestHelper.CreateStubShortUrlAsync(client,
            accessToken: user1Login.AccessToken, cancellationToken: cancellationToken);
        
        var user2Login = await AuthHelper.RegisterAndLoginAsync(client,
            "user2", Constants.TestPassword, cancellationToken);

        // Act
        var httpResponse = await client.SendAuthenticatedRequestAsJsonAsync(
            HttpMethod.Delete, $"/short-urls/{stubShortUrl.Id}", user2Login.AccessToken, cancellationToken);
        var problemDetails = await httpResponse.Content.ReadFromJsonAsync<ProblemDetails>(cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, httpResponse.StatusCode);
        var expected = ShortUrlError.NotFound.ToProblemDetails();
        AssertHelper.AssertProblemDetails(expected, problemDetails);
    }
}
