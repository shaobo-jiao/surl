using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Surl.Api;

public static class ClaimPrincipalExtensions
{   
    /// <summary>
    /// Extracts UserId from ClaimPrincipal given that the identity is authenticated.
    /// </summary>
    /// <param name="principal"></param>
    /// <returns></returns>
    public static int GetUserId(this ClaimsPrincipal principal)
    {
        if (!(principal.Identity?.IsAuthenticated ?? false))
            throw new InvalidOperationException();

        if (!int.TryParse(principal.FindFirstValue(JwtRegisteredClaimNames.Sub), out int userId))
            throw new InvalidOperationException();

        return userId;
    }
}
