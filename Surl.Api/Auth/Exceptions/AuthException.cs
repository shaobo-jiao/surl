namespace Surl.Api.Auth;

/// <summary>
/// Base class for authentication-related exceptions
/// </summary>
public class AuthException : Exception
{
    public AuthException(string message) : base(message) { }
}
