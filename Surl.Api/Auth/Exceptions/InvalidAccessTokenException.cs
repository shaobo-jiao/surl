namespace Surl.Api.Auth;

public class InvalidAccessTokenException : AuthException
{
    public InvalidAccessTokenException(string message = "Invalid access token")
        : base(message)
    {
    }
}
