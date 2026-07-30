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
        string username, string password = Constants.TestPassword, CancellationToken cancellationToken = default)
    {
        var registerRequest = new RegisterRequest(username, password);
        var httpResponse = await client.PostAsJsonAsync("/auth/register", registerRequest, cancellationToken);

        var userResponse = await httpResponse.Content.ReadFromJsonAsync<UserResponse>(cancellationToken)
            ?? throw new InvalidOperationException();
        return userResponse;
    }

    public static async Task<LoginResponse> LoginAsync(HttpClient client, 
        string username, string password = Constants.TestPassword, CancellationToken cancellationToken = default)
    {
        var loginRequest = new LoginRequest(username, password);
        var httpResponse = await client.PostAsJsonAsync("/auth/login", loginRequest, cancellationToken);

        var loginResponse = await httpResponse.Content.ReadFromJsonAsync<LoginResponse>(cancellationToken)
            ?? throw new InvalidOperationException();
        return loginResponse;
    }

    public static HttpRequestMessage CreateAuthenticatedHttpRequestMessageAsJson<T>(HttpMethod method, string uri, string accessToken, T value)
    {
        var message = new HttpRequestMessage(method, uri);
        message.Headers.Authorization = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, accessToken);
        message.Content = JsonContent.Create(value);
        return message;
    }
}
