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
        expiresAt ??= Constants.TestStartTime.AddMinutes(1);
        var createRequest = new CreateShortUrlRequest(originalUrl, expiresAt);

        var httpResponse = string.IsNullOrEmpty(accessToken)
            ? await client.PostAsJsonAsync("/short-urls", createRequest, cancellationToken)
            : await client.SendAuthenticatedRequestAsJsonAsync(HttpMethod.Post, "/short-urls",
                accessToken, createRequest, cancellationToken);

        return await httpResponse.Content.ReadFromJsonAsync<ShortUrlResponse>(cancellationToken)
            ?? throw new InvalidOperationException();
    }

    public static async Task CreateStubShortUrlsAsync(HttpClient client, 
        IEnumerable<CreateShortUrlRequest> createRequests, string? accessToken = null, 
        CancellationToken cancellationToken = default)
    {
        
    }
}
