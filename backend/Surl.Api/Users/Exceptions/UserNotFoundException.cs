namespace Surl.Api.Users;

public class UserNotFoundException : UserException
{
    public UserNotFoundException(string message = "User not found") : base(message)
    {
    }
}
