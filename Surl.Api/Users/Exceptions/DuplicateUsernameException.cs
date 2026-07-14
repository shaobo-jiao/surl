namespace Surl.Api.Users;

public class DuplicateUsernameException : UserException
{
    public DuplicateUsernameException(string message = "Duplicate Username") : base(message)
    {
    }
}
