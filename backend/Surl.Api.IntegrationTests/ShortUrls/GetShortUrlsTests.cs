using System.Net;
using System.Net.Http.Json;
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
    public async Task GetShortUrl_WithoutQueryParams_ShouldReturnDefaultPagedResult()
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

        var page = PagedResultConstants.DefaultPage;
        var pageSize = PagedResultConstants.DefaultPageSize;
        var items = shortUrls.OrderBy(x => x.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize);
        var expectedResult = new PagedResult<ShortUrlResponse>(
            items, page, pageSize, shortUrls.Count());

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

    public static List<CreateShortUrlRequest> CreateStubShortUrlRequests()
    {
        var requests = new List<CreateShortUrlRequest>();
        for (int i = 1; i <= 15; i++)
        {
            requests.Add(new("https://www.example.com", Constants.TestStartTime.AddDays(1)));
        }
        return requests;
    }

    // test 1: anonymous => unauthorized

    // test 2: no filter and other params => returns all

    // test 3: with filter => return filtered

    // test 4: with OrderBy = ExpiresAt => result ordered by the property

    // test 5: with OrderBy = ClickCount => result ordered by click count

    // test 6: with IsDescending = true => result in descending order

    // test 7: with Page and PageSize => get expected paged result
}
