using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Surl.Api.Common;
using Surl.Api.Data;
using Surl.Api.IntegrationTests.Helpers;
using Surl.Api.ShortUrls;

namespace Surl.Api.IntegrationTests.ShortUrls;

[Collection("SurlApiIntegrationTests")]
public class QueryShortUrlsTests(CustomWebApplicationFactory factory) : IAsyncLifetime
{
    private readonly CustomWebApplicationFactory _factory = factory;

    public ValueTask InitializeAsync() => ValueTask.CompletedTask;

    public async ValueTask DisposeAsync() => await _factory.ResetStateAsync();

    [Fact]
    public async Task QueryShortUrls_AnonymousRequest_ShouldReturnUnauthorized()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        using var client = _factory.CreateClient();

        // Act: query without any parameters
        var response = await client.GetAsync("/short-urls", cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task QueryShortUrls_WithoutQueryParams_ShouldReturnAllUrls()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        using var client = _factory.CreateClient();

        var userLogin = await AuthHelper.RegisterAndLoginAsync(client,
            TestConstants.TestUser1, TestConstants.TestPassword, cancellationToken);

        var data = CreateStubShortUrlRequests(numberOfExpiringUrls: 5, numberOfNeverExpiringUrls: 3);
        var shortUrls = await ShortUrlTestHelper.CreateStubShortUrlsAsync(
            client, data, userLogin.AccessToken, cancellationToken);

        // no expiry params: every url is returned, ordered by Id ASC
        var expectedShortUrls = shortUrls.OrderBy(x => x.Id);
        var expectedResult = ConvertToPagedResult(expectedShortUrls,
            PagedResultConstants.DefaultPage, PagedResultConstants.DefaultPageSize);

        // Act
        var httpResponse = await client.SendAuthenticatedRequestAsJsonAsync(
            HttpMethod.Get, "/short-urls", userLogin.AccessToken, cancellationToken);
        var queryResult = await httpResponse.Content.ReadFromJsonAsync<PagedResult<ShortUrlResponse>>(
            cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
        Assert.NotNull(queryResult);
        AssertHelper.AssertPagedResult(expectedResult, queryResult);
    }

    [Fact]
    public async Task QueryShortUrls_FilteringByBothExpiresFromAndExpiresTo_ShouldReturnUrlsInRange()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        using var client = _factory.CreateClient();

        var userLogin = await AuthHelper.RegisterAndLoginAsync(client,
            TestConstants.TestUser1, TestConstants.TestPassword, cancellationToken);
        var data = CreateStubShortUrlRequests();
        var shortUrls = await ShortUrlTestHelper.CreateStubShortUrlsAsync(
            client, data, userLogin.AccessToken, cancellationToken);

        var expiresFrom = TestConstants.TestStartTime.AddDays(1);
        var expiresTo = TestConstants.TestStartTime.AddDays(50);
        // both bounds given: only urls with an expiry inside the range; never-expiring urls are excluded
        var expectedShortUrls = shortUrls
            .Where(x => x.ExpiresAt >= expiresFrom && x.ExpiresAt <= expiresTo)
            .OrderBy(x => x.Id);
        var expectedResult = ConvertToPagedResult(expectedShortUrls,
            PagedResultConstants.DefaultPage, PagedResultConstants.DefaultPageSize);

        // Act
        var uri = QueryHelpers.AddQueryString("/short-urls", new Dictionary<string, string?>()
        {
            ["ExpiresFrom"] = expiresFrom.ToString("o"),
            ["ExpiresTo"] = expiresTo.ToString("o")
        });
        var httpResponse = await client.SendAuthenticatedRequestAsJsonAsync(
            HttpMethod.Get, uri, userLogin.AccessToken, cancellationToken);
        var queryResult = await httpResponse.Content.ReadFromJsonAsync<PagedResult<ShortUrlResponse>>(
            cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
        Assert.NotNull(queryResult);
        AssertHelper.AssertPagedResult(expectedResult, queryResult);
    }

    [Fact]
    public async Task QueryShortUrls_FilteringByExpiresFromOnly_ShouldIncludeNeverExpiringUrls()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        using var client = _factory.CreateClient();

        var userLogin = await AuthHelper.RegisterAndLoginAsync(client,
            TestConstants.TestUser1, TestConstants.TestPassword, cancellationToken);
        var data = CreateStubShortUrlRequests();
        var shortUrls = await ShortUrlTestHelper.CreateStubShortUrlsAsync(
            client, data, userLogin.AccessToken, cancellationToken);

        var expiresFrom = TestConstants.TestStartTime.AddDays(50);
        // only a lower bound given: never-expiring urls are unbounded, so they are included too
        var expectedShortUrls = shortUrls
            .Where(x => x.ExpiresAt is null || x.ExpiresAt >= expiresFrom)
            .OrderBy(x => x.Id);
        var expectedResult = ConvertToPagedResult(expectedShortUrls,
            PagedResultConstants.DefaultPage, PagedResultConstants.DefaultPageSize);

        // Act
        var uri = QueryHelpers.AddQueryString("/short-urls", new Dictionary<string, string?>()
        {
            ["ExpiresFrom"] = expiresFrom.ToString("o"),
        });
        var httpResponse = await client.SendAuthenticatedRequestAsJsonAsync(
            HttpMethod.Get, uri, userLogin.AccessToken, cancellationToken);
        var queryResult = await httpResponse.Content.ReadFromJsonAsync<PagedResult<ShortUrlResponse>>(
            cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
        Assert.NotNull(queryResult);
        AssertHelper.AssertPagedResult(expectedResult, queryResult);
    }

    [Fact]
    public async Task QueryShortUrls_FilteringByExpiresToOnly_ShouldExcludeNeverExpiringUrls()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        using var client = _factory.CreateClient();

        var userLogin = await AuthHelper.RegisterAndLoginAsync(client,
            TestConstants.TestUser1, TestConstants.TestPassword, cancellationToken);
        var data = CreateStubShortUrlRequests();
        var shortUrls = await ShortUrlTestHelper.CreateStubShortUrlsAsync(
            client, data, userLogin.AccessToken, cancellationToken);

        var expiresTo = TestConstants.TestStartTime.AddDays(50);
        // only an upper bound given: only urls expiring on or before it; never-expiring urls are excluded
        var expectedShortUrls = shortUrls
            .Where(x => x.ExpiresAt is not null && x.ExpiresAt <= expiresTo)
            .OrderBy(x => x.Id);
        var expectedResult = ConvertToPagedResult(expectedShortUrls,
            PagedResultConstants.DefaultPage, PagedResultConstants.DefaultPageSize);

        // Act
        var uri = QueryHelpers.AddQueryString("/short-urls", new Dictionary<string, string?>()
        {
            ["ExpiresTo"] = expiresTo.ToString("o"),
        });
        var httpResponse = await client.SendAuthenticatedRequestAsJsonAsync(
            HttpMethod.Get, uri, userLogin.AccessToken, cancellationToken);
        var queryResult = await httpResponse.Content.ReadFromJsonAsync<PagedResult<ShortUrlResponse>>(
            cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
        Assert.NotNull(queryResult);
        AssertHelper.AssertPagedResult(expectedResult, queryResult);
    }

    [Fact]
    public async Task QueryShortUrls_OrderbyExpiresAt_ShouldReturnOrderedResult()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        using var client = _factory.CreateClient();

        var userLogin = await AuthHelper.RegisterAndLoginAsync(client,
            TestConstants.TestUser1, TestConstants.TestPassword, cancellationToken);
        var data = CreateStubShortUrlRequests();
        var shortUrls = await ShortUrlTestHelper.CreateStubShortUrlsAsync(
            client, data, userLogin.AccessToken, cancellationToken);

        var expectedShortUrls = shortUrls
            .OrderBy(x => x.ExpiresAt is null) // sorts NULLs last for ASC, same as Postgres
            .ThenBy(x => x.ExpiresAt)
            .ThenBy(x => x.Id);
        var expectedResult = ConvertToPagedResult(expectedShortUrls,
            PagedResultConstants.DefaultPage, PagedResultConstants.DefaultPageSize);

        // Act
        var uri = QueryHelpers.AddQueryString("/short-urls", new Dictionary<string, string?>()
        {
            ["OrderBy"] = QueryShortUrlAllowedOrderByProperties.ExpiresAt.ToLowerInvariant(),
        });
        var httpResponse = await client.SendAuthenticatedRequestAsJsonAsync(
            HttpMethod.Get, uri, userLogin.AccessToken, cancellationToken);
        var queryResult = await httpResponse.Content.ReadFromJsonAsync<PagedResult<ShortUrlResponse>>(
            cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
        Assert.NotNull(queryResult);
        AssertHelper.AssertPagedResult(expectedResult, queryResult);
    }

    [Fact]
    public async Task QueryShortUrls_OrderbyExpiresAtDescending_ShouldReturnResultInDescendingOrder()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        using var client = _factory.CreateClient();

        var userLogin = await AuthHelper.RegisterAndLoginAsync(client,
            TestConstants.TestUser1, TestConstants.TestPassword, cancellationToken);
        var data = CreateStubShortUrlRequests();
        var shortUrls = await ShortUrlTestHelper.CreateStubShortUrlsAsync(
            client, data, userLogin.AccessToken, cancellationToken);

        var expectedShortUrls = shortUrls
            .OrderBy(x => x.ExpiresAt is not null) // sorts NULLs first for DESC, same as Postgres
            .ThenByDescending(x => x.ExpiresAt)
            .ThenByDescending(x => x.Id);
        var expectedResult = ConvertToPagedResult(expectedShortUrls,
            PagedResultConstants.DefaultPage, PagedResultConstants.DefaultPageSize);

        // Act
        var uri = QueryHelpers.AddQueryString("/short-urls", new Dictionary<string, string?>()
        {
            ["OrderBy"] = QueryShortUrlAllowedOrderByProperties.ExpiresAt,
            ["IsDescending"] = true.ToString(),
        });
        var httpResponse = await client.SendAuthenticatedRequestAsJsonAsync(
            HttpMethod.Get, uri, userLogin.AccessToken, cancellationToken);
        var queryResult = await httpResponse.Content.ReadFromJsonAsync<PagedResult<ShortUrlResponse>>(
            cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
        Assert.NotNull(queryResult);
        AssertHelper.AssertPagedResult(expectedResult, queryResult);
    }

    [Fact]
    public async Task QueryShortUrls_OrderbyClickCount_ShouldReturnOrderedResult()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        using var client = _factory.CreateClient();

        var userLogin = await AuthHelper.RegisterAndLoginAsync(client,
            TestConstants.TestUser1, TestConstants.TestPassword, cancellationToken);
        var data = CreateStubShortUrlRequests();
        var shortUrls = await ShortUrlTestHelper.CreateStubShortUrlsAsync(
            client, data, userLogin.AccessToken, cancellationToken);

        // generate click counts
        var randomClickCounts = RandomHelper.DistinctIntegers(shortUrls.Count(), 0, 100);
        shortUrls = shortUrls.Zip(randomClickCounts,
            (shortUrl, count) => shortUrl with { ClickCount = count });
        var clickCounts = shortUrls
            .Select(x => new KeyValuePair<string, long>(x.ShortUrlCode, x.ClickCount))
            .ToDictionary();
        await ShortUrlTestHelper.ClickShortUrlsAsync(client, clickCounts, cancellationToken);
        await ShortUrlTestHelper.WaitUntilAsync(async ct =>
        {
            await using var scope = _factory.Services.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var actual = await db.ShortUrlClicks.LongCountAsync(ct);
            var expected = clickCounts.Sum(p => p.Value);
            return actual == expected;
        });


        // ties are broken by Id ASC
        var expectedShortUrls = shortUrls.OrderBy(x => x.ClickCount).ThenBy(x => x.Id);
        var expectedResult = ConvertToPagedResult(expectedShortUrls,
            PagedResultConstants.DefaultPage, PagedResultConstants.DefaultPageSize);

        // Act
        var uri = QueryHelpers.AddQueryString("/short-urls", new Dictionary<string, string?>()
        {
            ["OrderBy"] = QueryShortUrlAllowedOrderByProperties.ClickCount.ToLowerInvariant(),
        });
        var httpResponse = await client.SendAuthenticatedRequestAsJsonAsync(
            HttpMethod.Get, uri, userLogin.AccessToken, cancellationToken);
        var queryResult = await httpResponse.Content.ReadFromJsonAsync<PagedResult<ShortUrlResponse>>(
            cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
        Assert.NotNull(queryResult);
        AssertHelper.AssertPagedResult(expectedResult, queryResult);
    }

    [Fact]
    public async Task QueryShortUrls_CustomPageAndPageSize_ShouldReturnExpectedPagedResult()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        using var client = _factory.CreateClient();

        var userLogin = await AuthHelper.RegisterAndLoginAsync(client,
            TestConstants.TestUser1, TestConstants.TestPassword, cancellationToken);
        var data = CreateStubShortUrlRequests();
        var shortUrls = await ShortUrlTestHelper.CreateStubShortUrlsAsync(
            client, data, userLogin.AccessToken, cancellationToken);

        var page = 2;
        var pageSize = 4;
        var expectedShortUrls = shortUrls.OrderBy(x => x.Id);
        var expectedResult = ConvertToPagedResult(expectedShortUrls, page, pageSize);

        // Act
        var uri = QueryHelpers.AddQueryString("/short-urls", new Dictionary<string, string?>()
        {
            ["Page"] = page.ToString(),
            ["PageSize"] = pageSize.ToString()
        });
        var httpResponse = await client.SendAuthenticatedRequestAsJsonAsync(
            HttpMethod.Get, uri, userLogin.AccessToken, cancellationToken);
        var queryResult = await httpResponse.Content.ReadFromJsonAsync<PagedResult<ShortUrlResponse>>(
            cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
        Assert.NotNull(queryResult);
        AssertHelper.AssertPagedResult(expectedResult, queryResult);
    }

    /// <summary>
    /// When urls tie on the primary ordering key, the Id secondary ordering must follow the
    /// requested direction. Never-expiring urls all tie on a null ExpiresAt, which makes them a
    /// convenient data set for this check.
    /// </summary>
    [Fact]
    public async Task QueryShortUrls_OrderbyExpiresAtWithTies_ShouldBreakTiesById()
    {
        // Arrange: only never-expiring urls, so every item ties on ExpiresAt
        var cancellationToken = TestContext.Current.CancellationToken;
        using var client = _factory.CreateClient();

        var userLogin = await AuthHelper.RegisterAndLoginAsync(client,
            TestConstants.TestUser1, TestConstants.TestPassword, cancellationToken);
        var data = CreateStubShortUrlRequests(numberOfExpiringUrls: 0, numberOfNeverExpiringUrls: 5);
        var shortUrls = await ShortUrlTestHelper.CreateStubShortUrlsAsync(
            client, data, userLogin.AccessToken, cancellationToken);

        // Act: ascending
        var uri = QueryHelpers.AddQueryString("/short-urls", new Dictionary<string, string?>()
        {
            ["OrderBy"] = QueryShortUrlAllowedOrderByProperties.ExpiresAt.ToLowerInvariant(),
        });
        var httpResponse = await client.SendAuthenticatedRequestAsJsonAsync(
            HttpMethod.Get, uri, userLogin.AccessToken, cancellationToken);
        var queryResult = await httpResponse.Content.ReadFromJsonAsync<PagedResult<ShortUrlResponse>>(
            cancellationToken);

        // Assert: ties are broken by Id ASC
        Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
        Assert.NotNull(queryResult);

        var expected = shortUrls.OrderBy(x => x.Id).Select(x => x.Id);
        Assert.Equal(expected, queryResult.Items.Select(x => x.Id));

        // Act: descending
        var descendingUri = QueryHelpers.AddQueryString("/short-urls", new Dictionary<string, string?>()
        {
            ["OrderBy"] = QueryShortUrlAllowedOrderByProperties.ExpiresAt,
            ["IsDescending"] = true.ToString(),
        });
        var descendingResponse = await client.SendAuthenticatedRequestAsJsonAsync(
            HttpMethod.Get, descendingUri, userLogin.AccessToken, cancellationToken);
        var descendingResult = await descendingResponse.Content.ReadFromJsonAsync<PagedResult<ShortUrlResponse>>(
            cancellationToken);

        // Assert: ties are broken by Id DESC
        Assert.Equal(HttpStatusCode.OK, descendingResponse.StatusCode);
        Assert.NotNull(descendingResult);
        expected = shortUrls.OrderByDescending(x => x.Id).Select(x => x.Id);
        Assert.Equal(expected, descendingResult.Items.Select(x => x.Id));
    }

    private static PagedResult<T> ConvertToPagedResult<T>(IEnumerable<T> items, int page, int pageSize)
    {
        var filteredItems = items
            .Skip((page - 1) * pageSize)
            .Take(pageSize);
        var expectedResult = new PagedResult<T>(
            filteredItems, page, pageSize, items.Count());
        return expectedResult;
    }

    /// <summary>
    /// creates a mix of never-expiring and expiring short urls as testing data
    /// </summary>
    private static List<CreateShortUrlRequest> CreateStubShortUrlRequests(
        int numberOfExpiringUrls = 8, int numberOfNeverExpiringUrls = 2)
    {
        var requests = new List<CreateShortUrlRequest>();

        // never-expiring urls: no expiry date is provided
        for (int i = 0; i < numberOfNeverExpiringUrls; i++)
            requests.Add(new("https://www.example.com"));

        // expiring urls: distinct expiry day offsets so the expiry ordering is deterministic
        var dayOffsets = RandomHelper.DistinctIntegers(numberOfExpiringUrls, 1, 100);
        foreach (var dayOffset in dayOffsets)
            requests.Add(new("https://www.example.com", TestConstants.TestStartTime.AddDays(dayOffset)));

        return requests;
    }
}
