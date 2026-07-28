namespace Surl.Api.Auth;

/// <summary>
/// Defines the errors returned by the Auth module.
/// </summary>
public enum AuthError
{
    DuplicateUsername,
    InvalidCredentials,
    InvalidRefreshToken,
    UserNotFound,
}
