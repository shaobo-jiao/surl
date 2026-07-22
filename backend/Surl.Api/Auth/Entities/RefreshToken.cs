using Surl.Api.Users;

namespace Surl.Api.Auth;

public class RefreshToken
{
    /// <summary>
    /// Id of a refresh token; primary key; auto populated from the database. 
    /// </summary>
    public int Id { get; private set; }

    /// <summary>
    /// The refresh token value
    /// </summary>
    public required string Token { get; set; }

    /// <summary>
    /// UserId that the refresh token is generated for
    /// </summary>
    public required int UserId { get; set; }
    
    public required DateTimeOffset ExpiresAt { get; set; }
    
    /// <summary>
    /// Created date time, it's required and set to time provider's current datetime. 
    /// </summary>
    public required DateTimeOffset CreatedAt { get; init; }
    
    /// <summary>
    /// Whether the refresh token has been used. Related endpoint: /auth/refresh
    /// </summary>
    public bool IsUsed { get; set; } = false;
    
    /// <summary>
    /// Whether the refresh token has been revoked. 
    /// A user's refresh tokens will be revoked after the user's password is changed.
    /// </summary>
    public bool IsRevoked { get; set; } = false;

    public User User { get; set; } = default!;

    /// <summary>
    /// Checks whether the refresh token is valid at given point in time.
    /// </summary>
    /// <param name="utcNow"></param>
    /// <returns></returns>
    public bool IsValidAt(DateTimeOffset utcNow) => !IsUsed && !IsRevoked && ExpiresAt > utcNow;
}
