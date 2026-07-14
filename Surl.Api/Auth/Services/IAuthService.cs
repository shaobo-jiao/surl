using Surl.Api.Users;

namespace Surl.Api.Auth;

public interface IAuthService
{
    Task<User> RegisterAsync(RegisterRequest registerRequest);
    Task<LoginResponse> LoginAsync(LoginRequest loginRequest);
    Task<RefreshResponse> RefreshAsync(RefreshRequest refreshRequest);
    Task ChangePasswordAsync(int userId, ChangePasswordRequest resetPasswordRequest);
}
