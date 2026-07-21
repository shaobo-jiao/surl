using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Surl.Api.Users;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Surl.Api.Auth;

public class JwtService(IOptions<JwtOptions> jwtOptions) : ITokenService
{
    private JwtOptions _jwtOptions = jwtOptions.Value;

    public string GenerateAccessToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SecretKey));
        var signingCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // identity related claims grouped together, later assign to Subject as one ClaimIdentity
        var identityClaims = new List<Claim>()
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Name, user.Username),
            new Claim(AppClaimTypes.Role, user.Role)
        };

        var tokenDescriptor = new SecurityTokenDescriptor()
        {
            Subject = new ClaimsIdentity(identityClaims),
            SigningCredentials = signingCredentials,

            Issuer = _jwtOptions.Issuer,
            Audience = _jwtOptions.Audience,

            NotBefore = DateTime.UtcNow,
            Expires = DateTime.UtcNow.AddMinutes(_jwtOptions.AccessTokenExpireInMinutes),
        };

        return new JsonWebTokenHandler().CreateToken(tokenDescriptor);
    }

    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }
}
