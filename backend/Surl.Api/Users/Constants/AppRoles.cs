namespace Surl.Api.Users;

public static class AppRoles
{
    /// <summary>
    /// administrator role that can manage users. 
    /// </summary>
    public const string Admin = "Admin";
    
    /// <summary>
    /// Normal user of the application.
    /// </summary>
    public const string User = "User";

    public static List<string> All() => new List<string>() { Admin, User };
}
