namespace Surl.Api.Auth;

public record ChangePasswordRequest(string OldPassword, string NewPassword);

