namespace Surl.Api.Auth;

public record RegisterRequest(string Username, string Password, string? Email = null);
