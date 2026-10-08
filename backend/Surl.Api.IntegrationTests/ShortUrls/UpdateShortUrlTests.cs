using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Surl.Api.IntegrationTests.Helpers;
using Surl.Api.ShortUrls;

namespace Surl.Api.IntegrationTests.ShortUrls;

[Collection("SurlApiIntegrationTests")]
public class UpdateShortUrlTests(CustomWebApplicationFactory factory) : IAsyncLifetime
{
    private readonly CustomWebApplicationFactory _factory = factory;
    public ValueTask InitializeAsync() => ValueTask.CompletedTask;

    public async ValueTask DisposeAsync() => await _factory.ResetStateAsync();

    [Fact]
    public async Task UpdateShortUrl_AnonymousRequest_ShouldReturnUnauthorized()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        using var client = _factory.CreateClient();
        var request = new UpdateShortUrlRequest(TestConstants.TestStartTime.AddDays(1));

        // Act
        var response = await client.PatchAsJsonAsync("/short-urls/1", request, cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task UpdateShortUrl_ValidRequest_ShouldUpdateSuccessfully()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        using var client = _factory.CreateClient();

        // create a short url
        var loginResponse = await AuthHelper.RegisterAndLoginAsync(client,
            TestConstants.TestUser1, TestConstants.TestPassword, cancellationToken);
        var accessToken = loginResponse.AccessToken;

        var stubShortUrl = await ShortUrlTestHelper.CreateStubShortUrlAsync(client,
            accessToken: loginResponse.AccessToken,
            cancellationToken: cancellationToken);

        // create update request
        var newExpiresAt = TestConstants.TestStartTime.AddDays(1);
        var updateRequest = new UpdateShortUrlRequest(newExpiresAt);

        // Act
        var httpResponse = await client.SendAuthenticatedRequestAsJsonAsync(
            HttpMethod.Patch, $"/short-urls/{stubShortUrl.Id}", accessToken, updateRequest, cancellationToken);
        var updateResponse = await httpResponse.Content.ReadFromJsonAsync<ShortUrlResponse>(cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
        Assert.NotNull(updateResponse);
        var expected = stubShortUrl with
        {
            ExpiresAt = newExpiresAt,
        };
        Assert.Equivalent(expected, updateResponse);
    }

    [Fact]
    public async Task UpdateShortUrl_NullExpiresAt_ShouldClearExpiry()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        using var client = _factory.CreateClient();

        // create a short url that expires
        var loginResponse = await AuthHelper.RegisterAndLoginAsync(client,
            TestConstants.TestUser1, TestConstants.TestPassword, cancellationToken);
        var accessToken = loginResponse.AccessToken;

        var stubShortUrl = await ShortUrlTestHelper.CreateStubShortUrlAsync(client,
            expiresAt: TestConstants.TestStartTime.AddDays(1),
            accessToken: accessToken,
            cancellationToken: cancellationToken);

        // request to clear the expiry so the short url never expires
        var updateRequest = new UpdateShortUrlRequest(ExpiresAt: null);

        // Act
        var httpResponse = await client.SendAuthenticatedRequestAsJsonAsync(
            HttpMethod.Patch, $"/short-urls/{stubShortUrl.Id}", accessToken, updateRequest, cancellationToken);
        var updateResponse = await httpResponse.Content.ReadFromJsonAsync<ShortUrlResponse>(cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
        Assert.NotNull(updateResponse);
        var expected = stubShortUrl with
        {
            ExpiresAt = null,
        };
        Assert.Equivalent(expected, updateResponse);
    }

    [Fact]
    public async Task UpdateShortUrl_NonExistentShortUrl_ShouldReturnNotFound()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        using var client = _factory.CreateClient();

        var loginResponse = await AuthHelper.RegisterAndLoginAsync(client,
            TestConstants.TestUser1, TestConstants.TestPassword, cancellationToken);
        var accessToken = loginResponse.AccessToken;

        var updateRequest = new UpdateShortUrlRequest(TestConstants.TestStartTime.AddDays(1));

        // Act
        var httpResponse = await client.SendAuthenticatedRequestAsJsonAsync(
            HttpMethod.Patch, "/short-urls/12345", accessToken, updateRequest, cancellationToken);
        var problemDetails = await httpResponse.Content.ReadFromJsonAsync<ProblemDetails>(cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, httpResponse.StatusCode);
        var expected = ShortUrlError.NotFound.ToProblemDetails();
        AssertHelper.AssertProblemDetails(expected, problemDetails);
    }

    [Fact]
    public async Task UpdateShortUrl_UpdatingUrlCreatedByOthers_ShouldReturnNotFound()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        using var client = _factory.CreateClient();

        // create a short url for user1
        var user1Login = await AuthHelper.RegisterAndLoginAsync(client,
            TestConstants.TestUser1, TestConstants.TestPassword, cancellationToken);
        var stubShortUrl = await ShortUrlTestHelper.CreateStubShortUrlAsync(client,
            accessToken: user1Login.AccessToken,
            cancellationToken: cancellationToken);

        // try update the url as user2
        var user2Login = await AuthHelper.RegisterAndLoginAsync(client,
            TestConstants.TestUser2, TestConstants.TestPassword, cancellationToken);
        var updateRequest = new UpdateShortUrlRequest(TestConstants.TestStartTime.AddDays(1));

        // Act
        var httpResponse = await client.SendAuthenticatedRequestAsJsonAsync(
            HttpMethod.Patch, $"/short-urls/{stubShortUrl.Id}", user2Login.AccessToken, updateRequest, cancellationToken);
        var problemDetails = await httpResponse.Content.ReadFromJsonAsync<ProblemDetails>(cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, httpResponse.StatusCode);
        var expected = ShortUrlError.NotFound.ToProblemDetails();
        AssertHelper.AssertProblemDetails(expected, problemDetails);
    }

    [Fact]
    public async Task UpdateShortUrl_InvalidRequestFields_ShouldReturnValidationFailureAsync()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        using var client = _factory.CreateClient();

        // create a short url
        var loginResponse = await AuthHelper.RegisterAndLoginAsync(client,
            TestConstants.TestUser1, TestConstants.TestPassword, cancellationToken);
        var accessToken = loginResponse.AccessToken;

        var stubShortUrl = await ShortUrlTestHelper.CreateStubShortUrlAsync(client,
            accessToken: loginResponse.AccessToken,
            cancellationToken: cancellationToken);

        // create update request
        var newExpiresAt = TestConstants.TestStartTime.AddSeconds(-1);
        var updateRequest = new UpdateShortUrlRequest(newExpiresAt);

        // Act
        var httpResponse = await client.SendAuthenticatedRequestAsJsonAsync(
            HttpMethod.Patch, $"/short-urls/{stubShortUrl.Id}", accessToken, updateRequest, cancellationToken);
        var validationDetails = await httpResponse.Content.ReadFromJsonAsync<ValidationProblemDetails>(cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, httpResponse.StatusCode);
        Assert.NotNull(validationDetails);
        Assert.Equivalent(new HashSet<string> { "expiresAt" }, 
            validationDetails.Errors.Keys);
    }
}
