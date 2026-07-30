using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Surl.Api.Auth;
using Surl.Api.IntegrationTests.Helpers;
using Surl.Api.Users;

namespace Surl.Api.IntegrationTests.Auth;

[Collection("SurlApiIntegrationTests")]
public class RegisterTests(CustomWebApplicationFactory factory) : IAsyncLifetime
{
    #region Life Cycle
    public readonly CustomWebApplicationFactory _factory = factory;

    public ValueTask InitializeAsync() => ValueTask.CompletedTask;

    public async ValueTask DisposeAsync() => await _factory.ResetStateAsync();
    #endregion


    [Fact]
    public async Task Register_ValidRequest_ShouldSucceed()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var client = _factory.HttpClient;
        var registerRequest = new RegisterRequest("user1", Constants.TestPassword, "a@a.com");

        // Act
        var httpResponse = await client.PostAsJsonAsync("/auth/register", registerRequest, cancellationToken);
        var userResponse = await httpResponse.Content.ReadFromJsonAsync<UserResponse>(cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Created, httpResponse.StatusCode);
        Assert.NotNull(userResponse);
        var expected = new UserResponse(default, "user1", "a@a.com", default);

        Assert.EquivalentWithExclusions(expected, userResponse,
            user => user.Id,
            user => user.CreatedAt);
    }

    [Fact]
    public async Task Register_DuplicateUserName_ShouldReturnBadRequest()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var client = _factory.HttpClient;

        await AuthHelper.RegisterAsync(client, "user1", Constants.TestPassword, cancellationToken);
        var registerRequest = new RegisterRequest("user1", Constants.TestPassword, "a@a.com");

        // Act
        var httpResponse = await client.PostAsJsonAsync("/auth/register", registerRequest, cancellationToken);
        var problemDetails = await httpResponse.Content.ReadFromJsonAsync<ProblemDetails>(cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, httpResponse.StatusCode);
        
        var expected = AuthError.DuplicateUsername.ToProblemDetails();
        AssertProblemDetailsHelper.AssertProblemDetails(expected, problemDetails);
    }

    // <summary>
    /// There's only one method here to test the request validator is working. 
    /// Detailed testing on the request validator are in unit tests. 
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task Register_InvalidRequestFields_ShouldReturnValidationFailure()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var client = _factory.HttpClient;

        var registerRequest = new RegisterRequest("user@#$", "123", "INVALID_EMAIL");

        // Act
        var httpResponse = await client.PostAsJsonAsync("/auth/register", registerRequest, cancellationToken);
        var validationDetails = await httpResponse.Content.ReadFromJsonAsync<ValidationProblemDetails>(cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, httpResponse.StatusCode);
        Assert.NotNull(validationDetails);
        Assert.Equivalent(new HashSet<string> { "Username", "Password", "Email" }, 
            validationDetails.Errors.Keys);
    }
}
