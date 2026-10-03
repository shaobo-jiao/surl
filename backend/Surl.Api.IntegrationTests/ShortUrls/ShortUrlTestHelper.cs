using System.Net.Http.Json;
using Surl.Api.IntegrationTests.Helpers;
using Surl.Api.ShortUrls;

namespace Surl.Api.IntegrationTests.ShortUrls;

public static class ShortUrlTestHelper
{
    public static async Task<ShortUrlResponse> CreateStubShortUrlAsync(HttpClient client,
        string? originalUrl = null, DateTimeOffset? expiresAt = null,
        string? accessToken = null, CancellationToken cancellationToken = default)
    {
        originalUrl ??= "https://www.google.com";
        expiresAt ??= TestConstants.TestStartTime.AddMinutes(1);
        var createRequest = new CreateShortUrlRequest(originalUrl, expiresAt);

        var httpResponse = string.IsNullOrEmpty(accessToken)
            ? await client.PostAsJsonAsync("/short-urls/anonymous", createRequest, cancellationToken)
            : await client.SendAuthenticatedRequestAsJsonAsync(HttpMethod.Post, "/short-urls",
                accessToken, createRequest, cancellationToken);

        return await httpResponse.Content.ReadFromJsonAsync<ShortUrlResponse>(cancellationToken)
            ?? throw new InvalidOperationException();
    }

    /// <summary>
    /// Creates a list of short URLs for testing. 
    /// </summary>
    /// <param name="client"></param>
    /// <param name="createRequests"></param>
    /// <param name="accessToken"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<IEnumerable<ShortUrlResponse>> CreateStubShortUrlsAsync(HttpClient client,
        IEnumerable<CreateShortUrlRequest> createRequests, 
        string? accessToken = null, CancellationToken cancellationToken = default)
    {
        var responses = new List<ShortUrlResponse>();
        foreach (var request in createRequests)
        {
            var response = await CreateStubShortUrlAsync(client, request.OriginalUrl, request.ExpiresAt,
                accessToken, cancellationToken);
            responses.Add(response);
        }
        return responses;
    }

    /// <summary>
    /// Clicks the given short url by given times 
    /// </summary>
    /// <param name="client"></param>
    /// <param name="shortUrlCode"></param>
    /// <param name="count"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task ClickShortUrlAsync(HttpClient client, string shortUrlCode, long count = 1, 
        CancellationToken cancellationToken = default)
    {
        for (int i = 0; i < count; i++)
        {
            await client.GetAsync($"/redirection/{shortUrlCode}", cancellationToken);
        }
    }

    /// <summary>
    /// Clicks the given list of short urls by corresponding times
    /// </summary>
    /// <param name="client"></param>
    /// <param name="shortUrlClicks"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task ClickShortUrlsAsync(HttpClient client, Dictionary<string, long> shortUrlClicks, 
        CancellationToken cancellationToken)
    {
        foreach (var pair in shortUrlClicks)
        {
            await ClickShortUrlAsync(client, pair.Key, pair.Value, cancellationToken);
        }
    }
}
