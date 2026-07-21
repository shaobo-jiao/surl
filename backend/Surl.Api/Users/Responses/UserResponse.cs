namespace Surl.Api.Users;

public record UserResponse(int Id, string Username, string? Email, DateTimeOffset CreatedAt);

public static partial class UserMapper
{
    public static UserResponse ToResponse(this User user) => new UserResponse(user.Id, user.Username, user.Email, user.CreatedAt);
}