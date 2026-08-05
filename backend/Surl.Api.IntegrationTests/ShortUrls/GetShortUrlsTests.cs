using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.WebUtilities;
using Surl.Api.Common;
using Surl.Api.IntegrationTests.Helpers;
using Surl.Api.ShortUrls;

namespace Surl.Api.IntegrationTests.ShortUrls;

[Collection("SurlApiIntegrationTests")]
public class GetShortUrlsTests(CustomWebApplicationFactory factory) : IAsyncLifetime
{
    private readonly CustomWebApplicationFactory _factory = factory;

    public ValueTask InitializeAsync() => ValueTask.CompletedTask;

    public async ValueTask DisposeAsync() => await _factory.ResetStateAsync();

    [Fact]
    public async Task GetShortUrls_AnonymousRequest_ShouldReturnUnauthorized()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var client = _factory.HttpClient;

        // Act: query without any parameters
        var response = await client.GetAsync("/short-urls", cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetShortUrls_WithoutQueryParams_ShouldReturnDefaultPagedResult()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var client = _factory.HttpClient;

        // prepare data
        var userLogin = await AuthHelper.RegisterAndLoginAsync(client,
            Constants.TestUser, Constants.TestPassword, cancellationToken);
        var data = CreateStubShortUrlRequests();
        var shortUrls = await ShortUrlTestHelper.CreateStubShortUrlsAsync(
            client, data, userLogin.AccessToken, cancellationToken);

        shortUrls = shortUrls.OrderBy(x => x.Id);
        var expectedResult = ConvertToPagedResult(shortUrls,
            PagedResultConstants.DefaultPage, PagedResultConstants.DefaultPageSize);

        // Act
        var httpResponse = await client.SendAuthenticatedRequestAsJsonAsync(
            HttpMethod.Get, "/short-urls", userLogin.AccessToken, cancellationToken);
        var queryResult = await httpResponse.Content.ReadFromJsonAsync<PagedResult<ShortUrlResponse>>(
            cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
        Assert.NotNull(queryResult);

        Assert.Equal(expectedResult.Page, queryResult.Page);
        Assert.Equal(expectedResult.PageSize, queryResult.PageSize);
        Assert.Equal(expectedResult.TotalCount, queryResult.TotalCount);
        Assert.Equal(expectedResult.TotalPages, queryResult.TotalPages);
        Assert.Equal(expectedResult.Items, queryResult.Items);
    }

    [Fact]
    public async Task GetShortUrls_FilteringExpiresAt_ShouldReturnFilteredResult()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var client = _factory.HttpClient;

        // prepare data
        var userLogin = await AuthHelper.RegisterAndLoginAsync(client,
            Constants.TestUser, Constants.TestPassword, cancellationToken);
        var data = CreateStubShortUrlRequests();
        var shortUrls = await ShortUrlTestHelper.CreateStubShortUrlsAsync(
            client, data, userLogin.AccessToken, cancellationToken);

        var expiresFrom = Constants.TestStartTime.AddDays(1);
        var expiresTo = Constants.TestStartTime.AddDays(50);
        var expectedShortUrls = shortUrls
            .Where(x => x.ExpiresAt >= expiresFrom)
            .Where(x => x.ExpiresAt <= expiresTo)
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
        AssertPagedResult(expectedResult, queryResult);
    }


    [Fact]
    public async Task GetShortUrls_OrderbyExpiresAt_ShouldReturnOrderedResult()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var client = _factory.HttpClient;

        // prepare data
        var userLogin = await AuthHelper.RegisterAndLoginAsync(client,
            Constants.TestUser, Constants.TestPassword, cancellationToken);
        var data = CreateStubShortUrlRequests();
        var shortUrls = await ShortUrlTestHelper.CreateStubShortUrlsAsync(
            client, data, userLogin.AccessToken, cancellationToken);

        var expectedShortUrls = shortUrls.OrderBy(x => x.ExpiresAt);
        var expectedResult = ConvertToPagedResult(expectedShortUrls,
            PagedResultConstants.DefaultPage, PagedResultConstants.DefaultPageSize);

        // Act
        var uri = QueryHelpers.AddQueryString("/short-urls", new Dictionary<string, string?>()
        {
            ["OrderBy"] = QueryShortUrlAllowedOrderByProperties.ExpiresAt,
        });
        var httpResponse = await client.SendAuthenticatedRequestAsJsonAsync(
            HttpMethod.Get, uri, userLogin.AccessToken, cancellationToken);
        var queryResult = await httpResponse.Content.ReadFromJsonAsync<PagedResult<ShortUrlResponse>>(
            cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
        Assert.NotNull(queryResult);
        AssertPagedResult(expectedResult, queryResult);
    }

    [Fact]
    public async Task GetShortUrls_OrderbyClickCount_ShouldReturnOrderedResult()
    {
        // TODO:
        // - add ClickCount in ShortUrlResponse
        // - find a way to efficiently return the click counts
        // - RandomHelper with Distinct ints/longs
        // - generate random clicks and test OrderByClickCount
    }

    [Fact]
    public async Task GetShortUrls_OrderbyExpiresAtDescending_ShouldReturnResultInDescendingOrder()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var client = _factory.HttpClient;

        // prepare data
        var userLogin = await AuthHelper.RegisterAndLoginAsync(client,
            Constants.TestUser, Constants.TestPassword, cancellationToken);
        var data = CreateStubShortUrlRequests();
        var shortUrls = await ShortUrlTestHelper.CreateStubShortUrlsAsync(
            client, data, userLogin.AccessToken, cancellationToken);

        var expectedShortUrls = shortUrls.OrderByDescending(x => x.ExpiresAt);
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
        AssertPagedResult(expectedResult, queryResult);
    }

    [Fact]
    public async Task GetShortUrls_CustomPageAndPageSize_ShouldReturnExpectedPagedResult()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var client = _factory.HttpClient;

        // prepare data
        var userLogin = await AuthHelper.RegisterAndLoginAsync(client,
            Constants.TestUser, Constants.TestPassword, cancellationToken);
        var data = CreateStubShortUrlRequests();
        var shortUrls = await ShortUrlTestHelper.CreateStubShortUrlsAsync(
            client, data, userLogin.AccessToken, cancellationToken);

        var page = 4;
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
        AssertPagedResult(expectedResult, queryResult);
    }


    private static void AssertPagedResult<T>(PagedResult<T> expected, PagedResult<T> actual)
    {
        Assert.Equal(expected.Page, actual.Page);
        Assert.Equal(expected.PageSize, actual.PageSize);
        Assert.Equal(expected.TotalCount, actual.TotalCount);
        Assert.Equal(expected.TotalPages, actual.TotalPages);
        Assert.Equal(expected.Items, actual.Items);
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

    private static List<CreateShortUrlRequest> CreateStubShortUrlRequests()
    {
        var numberOfUrls = 15;

        // prepare some random unique expiry day offsets;
        var daySet = new HashSet<int>();
        while (daySet.Count < numberOfUrls)
        {
            daySet.Add(Random.Shared.Next(1, 100));
        }
        var dayList = daySet.ToList();

        var requests = new List<CreateShortUrlRequest>();
        for (int i = 0; i < numberOfUrls; i++)
        {
            requests.Add(new("https://www.example.com", Constants.TestStartTime.AddDays(dayList[i])));
        }
        return requests;
    }

}
