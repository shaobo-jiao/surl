using Surl.Api.Common;
using Surl.Api.Users;

namespace Surl.Api.Auth;

public interface IAuthService
{
    Task<Result<User, AuthError>> RegisterAsync(RegisterInput registerInput, CancellationToken cancellationToken);

    Task<Result<LoginResult, AuthError>> LoginAsync(LoginInput loginInput, CancellationToken cancellationToken);
    
    /// <summary>
    /// Refresh the access token upon expiry using refresh token. 
    /// The refresh token will be marked used and can no longer be used afterwards. 
    /// </summary>
    /// <param name="refreshRequest"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>New access token and refresh token</returns>
    Task<Result<RefreshResult, AuthError>> RefreshAsync(string refreshToken, CancellationToken cancellationToken);

    Task<Result<RefreshToken, AuthError>> LogoutAsync(string refreshToken, CancellationToken cancellationToken);

    /// <summary>
    /// Changes password for an authenticated user. 
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="changePasswordRequest"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<Result<User, AuthError>> ChangePasswordAsync(int userId, ChangePasswordRequest changePasswordRequest, CancellationToken cancellationToken);
}
