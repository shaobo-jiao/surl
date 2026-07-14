namespace Surl.Api.Auth;

public class InvalidRefreshTokenException : AuthException
{
    public InvalidRefreshTokenException(string message = "Invalid refresh token")
        : base(message)
    {
    }
}
