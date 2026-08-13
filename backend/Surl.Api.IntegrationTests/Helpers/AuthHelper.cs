using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Surl.Api.Auth;
using Surl.Api.Users;

namespace Surl.Api.IntegrationTests.Helpers;

/// <summary>
/// Provides helper methods for tests that require auth feature
/// </summary>
public static class AuthHelper
{
    public static async Task<UserResponse> RegisterAsync(HttpClient client,
        string username, string password = TestConstants.TestPassword, CancellationToken cancellationToken = default)
    {
        var registerRequest = new RegisterRequest(username, password);
        var httpResponse = await client.PostAsJsonAsync("/auth/register", registerRequest, cancellationToken);

        var userResponse = await httpResponse.Content.ReadFromJsonAsync<UserResponse>(cancellationToken)
            ?? throw new InvalidOperationException();
        return userResponse;
    }

    public static async Task<LoginResponse> LoginAsync(HttpClient client,
        string username, string password = TestConstants.TestPassword, CancellationToken cancellationToken = default)
    {
        var loginRequest = new LoginRequest(username, password);
        var httpResponse = await client.PostAsJsonAsync("/auth/login", loginRequest, cancellationToken);

        var loginResponse = await httpResponse.Content.ReadFromJsonAsync<LoginResponse>(cancellationToken)
            ?? throw new InvalidOperationException();
        return loginResponse;
    }

    public static async Task LogoutAsync(HttpClient client, CancellationToken cancellationToken = default)
    {
        var httpResponse = await client.PostAsync("/auth/logout", null, cancellationToken);
        if (httpResponse.StatusCode != System.Net.HttpStatusCode.OK)
            throw new InvalidOperationException();
    }

    public static async Task<LoginResponse> RegisterAndLoginAsync(HttpClient client,
        string username, string password = TestConstants.TestPassword, CancellationToken cancellationToken = default)
    {
        await RegisterAsync(client, username, password, cancellationToken);
        return await LoginAsync(client, username, password, cancellationToken);
    }

    public static HttpRequestMessage CreateAuthenticatedHttpRequestMessageAsJson<T>(HttpMethod method, string uri, string accessToken, T value)
    {
        var message = new HttpRequestMessage(method, uri);
        message.Headers.Authorization = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, accessToken);
        message.Content = JsonContent.Create(value);
        return message;
    }

    public static async Task<HttpResponseMessage> SendAuthenticatedRequestAsJsonAsync<T>(
        this HttpClient client, HttpMethod method, string uri, string accessToken, T value,
        CancellationToken cancellationToken = default)
    {
        var message = new HttpRequestMessage(method, uri);
        message.Headers.Authorization = new AuthenticationHeaderValue(
            JwtBearerDefaults.AuthenticationScheme, accessToken);
        if (value is not null)
            message.Content = JsonContent.Create(value);
        return await client.SendAsync(message, cancellationToken);
    }

    public static async Task<HttpResponseMessage> SendAuthenticatedRequestAsJsonAsync(
        this HttpClient client, HttpMethod method, string uri, string accessToken,
        CancellationToken cancellationToken = default)
    {
        var message = new HttpRequestMessage(method, uri);
        message.Headers.Authorization = new AuthenticationHeaderValue(
            JwtBearerDefaults.AuthenticationScheme, accessToken);
        return await client.SendAsync(message, cancellationToken);
    }
}
