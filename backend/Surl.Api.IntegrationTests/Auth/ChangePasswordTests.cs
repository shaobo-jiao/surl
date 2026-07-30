using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Surl.Api.Auth;
using Surl.Api.IntegrationTests.Helpers;

namespace Surl.Api.IntegrationTests.Auth;

[Collection("SurlApiIntegrationTests")]
public class ChangePasswordTests(CustomWebApplicationFactory factory) : IAsyncLifetime
{
    #region Life Cycle
    private readonly CustomWebApplicationFactory _factory = factory;

    public ValueTask InitializeAsync() => ValueTask.CompletedTask;

    public async ValueTask DisposeAsync() => await _factory.ResetStateAsync();
    #endregion

    [Fact]
    public async Task ChangePassword_ValidRequest_ShouldChangePassword()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var client = _factory.HttpClient;
        
        const string username = "user1";
        const string newPassword = "NewP@ssw0rd123";
        var loginResponse = await AuthHelper.RegisterAndLoginAsync(client, 
            username, Constants.TestPassword, cancellationToken);

        var changePasswordRequest = new ChangePasswordRequest(Constants.TestPassword, newPassword);
        using var request = AuthHelper.CreateAuthenticatedHttpRequestMessageAsJson(
            HttpMethod.Post, "/auth/change-password", loginResponse.AccessToken, changePasswordRequest);

        // Act
        var httpResponse = await client.SendAsync(request, cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);

        var oldPasswordLoginResponse = await client.PostAsJsonAsync("/auth/login", 
            new LoginRequest(username, Constants.TestPassword), cancellationToken);
        Assert.Equal(HttpStatusCode.Unauthorized, oldPasswordLoginResponse.StatusCode);

        var newPasswordLoginResponse = await client.PostAsJsonAsync(
            "/auth/login", new LoginRequest(username, newPassword), cancellationToken);
        Assert.Equal(HttpStatusCode.OK, newPasswordLoginResponse.StatusCode);
    }

    [Fact]
    public async Task ChangePassword_AnonymousRequest_ShouldReturnUnauthorized()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var client = _factory.HttpClient;
        var changePasswordRequest = new ChangePasswordRequest(Constants.TestPassword, "NewPassword123");

        // Act
        var httpResponse = await client.PostAsJsonAsync("/auth/change-password", 
            changePasswordRequest, cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, httpResponse.StatusCode);
    }

    [Fact]
    public async Task ChangePassword_WrongOldPassword_ShouldReturnUnauthorized()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var client = _factory.HttpClient;

        var loginResponse = await AuthHelper.RegisterAndLoginAsync(
            client, "user1", Constants.TestPassword, cancellationToken);

        var changePasswordRequest = new ChangePasswordRequest("WrongPassword123", "NewP@ssw0rd123");
        using var request = AuthHelper.CreateAuthenticatedHttpRequestMessageAsJson(
            HttpMethod.Post, "/auth/change-password", loginResponse.AccessToken, changePasswordRequest);

        // Act
        var httpResponse = await client.SendAsync(request, cancellationToken);
        var problemDetails = await httpResponse.Content.ReadFromJsonAsync<ProblemDetails>(cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, httpResponse.StatusCode);
        var expected = AuthError.InvalidCredentials.ToProblemDetails();
        AssertProblemDetailsHelper.AssertProblemDetails(expected, problemDetails);
    }

    /// <summary>
    /// Only verifies that the request validator is active. Detailed validator tests belong in unit tests.
    /// </summary>
    [Fact]
    public async Task ChangePassword_InvalidRequestFields_ShouldReturnValidationFailure()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var client = _factory.HttpClient;
        var loginResponse = await AuthHelper.RegisterAndLoginAsync(client, 
            "user1", Constants.TestPassword, cancellationToken);

        var changePasswordRequest = new ChangePasswordRequest("123", "456");
        using var request = AuthHelper.CreateAuthenticatedHttpRequestMessageAsJson(
            HttpMethod.Post, "/auth/change-password", loginResponse.AccessToken, changePasswordRequest);

        // Act
        var httpResponse = await client.SendAsync(request, cancellationToken);
        var validationDetails = await httpResponse.Content.ReadFromJsonAsync<ValidationProblemDetails>(cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, httpResponse.StatusCode);
        Assert.NotNull(validationDetails);
        Assert.Equivalent(new HashSet<string> { "OldPassword", "NewPassword" }, 
            validationDetails.Errors.Keys);
    }
}
