using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Surl.Api.Auth;
using Surl.Api.IntegrationTests.Auth.Helpers;
using Surl.Api.IntegrationTests.Helpers;

namespace Surl.Api.IntegrationTests.Auth;

[Collection("SurlApiIntegrationTests")]
public class LoginTests(CustomWebApplicationFactory factory) : IAsyncLifetime
{
    private readonly CustomWebApplicationFactory _factory = factory;

    public ValueTask InitializeAsync() => ValueTask.CompletedTask;

    public async ValueTask DisposeAsync() => await _factory.ResetStateAsync();

    [Fact]
    public async Task Login_ValidCredentials_ShouldSucceed()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        using var client = _factory.CreateClient();

        await AuthHelper.RegisterAsync(client, Constants.TestUser1, Constants.TestPassword, cancellationToken);
        var loginRequest = new LoginRequest(Constants.TestUser1, Constants.TestPassword);
        
        // Act
        var httpResponse = await client.PostAsJsonAsync("/auth/login", loginRequest, cancellationToken);
        var loginResponse = await httpResponse.Content.ReadFromJsonAsync<LoginResponse>(cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
        Assert.NotNull(loginResponse);
        Assert.NotEmpty(loginResponse.AccessToken);
        AuthTestHelper.AssertRefreshTokenCookieSet(httpResponse, Constants.TestStartTime);
    }

    [Fact]
    public async Task Login_UsernameNotFound_ShouldReturnUnauthorized()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        using var client = _factory.CreateClient();
        var loginRequest = new LoginRequest("unknown-user", Constants.TestPassword);

        // Act
        var httpResponse = await client.PostAsJsonAsync("/auth/login", loginRequest, cancellationToken);
        var problemDetails = await httpResponse.Content.ReadFromJsonAsync<ProblemDetails>(cancellationToken);

        // Assert        
        Assert.Equal(HttpStatusCode.Unauthorized, httpResponse.StatusCode);
        var expected = AuthError.InvalidCredentials.ToProblemDetails();
        AssertHelper.AssertProblemDetails(expected, problemDetails);
    }

    [Fact]
    public async Task Login_WrongPassword_ShouldReturnUnauthorized()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        using var client = _factory.CreateClient();

        await AuthHelper.RegisterAsync(client, Constants.TestUser1, Constants.TestPassword, cancellationToken);
        var loginRequest = new LoginRequest(Constants.TestUser1, "WrongPassword123");

        // Act
        var httpResponse = await client.PostAsJsonAsync("/auth/login", loginRequest, cancellationToken);
        var problemDetails = await httpResponse.Content.ReadFromJsonAsync<ProblemDetails>(cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, httpResponse.StatusCode);
        var expected = AuthError.InvalidCredentials.ToProblemDetails();
        AssertHelper.AssertProblemDetails(expected, problemDetails);
    }

    /// <summary>
    /// Only verifies that the request validator is active. Detailed validator tests belong in unit tests.
    /// </summary>
    [Fact]
    public async Task Login_InvalidRequestFields_ShouldReturnValidationFailure()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        using var client = _factory.CreateClient();
        var loginRequest = new LoginRequest("user@#$", "123");

        // Act
        var httpResponse = await client.PostAsJsonAsync("/auth/login", loginRequest, cancellationToken);
        var validationDetails = await httpResponse.Content.ReadFromJsonAsync<ValidationProblemDetails>(cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, httpResponse.StatusCode);
        Assert.NotNull(validationDetails);
        Assert.Equivalent(new HashSet<string> { "Username", "Password" }, validationDetails.Errors.Keys);
    }
}
