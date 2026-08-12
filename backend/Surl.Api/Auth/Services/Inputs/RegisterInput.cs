namespace Surl.Api.Auth;

public record RegisterInput(string Username, string Password, string? Email = null);
