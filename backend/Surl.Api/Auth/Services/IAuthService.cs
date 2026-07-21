using Surl.Api.Users;

namespace Surl.Api.Auth;

public interface IAuthService
{
    /// <summary>
    /// Registers an application user
    /// </summary>
    /// <param name="registerRequest"></param>
    /// <returns>The created application user entity</returns>
    Task<User> RegisterAsync(RegisterRequest registerRequest);

    /// <summary>
    /// Logins to the application
    /// </summary>
    /// <param name="loginRequest"></param>
    /// <returns>Access token and refresh token</returns>
    Task<LoginResponse> LoginAsync(LoginRequest loginRequest);

    /// <summary>
    /// Refresh the access token upon expiry using refresh token. 
    /// The refresh token will be marked used and can no longer be used afterwards. 
    /// </summary>
    /// <param name="refreshRequest"></param>
    /// <returns>New access token and refresh token</returns>
    Task<RefreshResponse> RefreshAsync(RefreshRequest refreshRequest);

    /// <summary>
    /// Changes password for an authenticated user. 
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="resetPasswordRequest">The user's old and new password</param>
    /// <returns></returns>
    Task ChangePasswordAsync(int userId, ChangePasswordRequest resetPasswordRequest);
}
