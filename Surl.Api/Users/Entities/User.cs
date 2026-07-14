namespace Surl.Api.Users;

public class User
{
    public int Id { get; private set; }
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? Email { get; set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public string Role { get; set; } = Roles.User;

    private User() { }

    public User(string username) => Username = username;
}
