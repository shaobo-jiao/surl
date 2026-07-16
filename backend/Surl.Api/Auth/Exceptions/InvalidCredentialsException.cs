namespace Surl.Api.Auth;

public class InvalidCredentialsException : AuthException
{
    public InvalidCredentialsException(string message = "Invalid credential") : base(message)
    {
    }
}
