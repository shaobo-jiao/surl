namespace Surl.Api.Users;

public record CreateUserRequest(string Username, string Password, string? Email = null, string Role = Roles.User);

