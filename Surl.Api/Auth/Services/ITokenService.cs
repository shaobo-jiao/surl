using Surl.Api.Users;

namespace Surl.Api.Auth;

public interface ITokenService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
}
