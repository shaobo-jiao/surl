namespace Surl.Api.Users;

public record UpdateUserRequest(string Username, string Password, string? Email = null);
