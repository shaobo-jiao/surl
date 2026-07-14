namespace Surl.Api.Users;

public static class Roles
{
    public const string Admin = "Admin";
    public const string User = "User";

    public static List<string> All() => new List<string>() { Admin, User };
}
