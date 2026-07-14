using Surl.Api.Users;

namespace Surl.Api.Auth;

public class RefreshToken
{
    public int Id { get; private set; }
    public required string Token { get; set; }
    public required int UserId { get; set; }
    public required DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public bool IsUsed { get; set; } = false;
    public bool IsRevoked { get; set; } = false;

    public User User { get; set; } = default!;

    public bool IsValid => !IsUsed && !IsRevoked && ExpiresAt >= DateTime.UtcNow;
}
