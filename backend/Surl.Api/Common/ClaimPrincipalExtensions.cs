using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Surl.Api;

public static class ClaimPrincipalExtensions
{   
    /// <summary>
    /// Extracts UserId from ClaimPrincipal
    /// </summary>
    /// <param name="principal"></param>
    /// <returns></returns>
    public static int? GetUserId(this ClaimsPrincipal principal)
    {
        if (int.TryParse(principal.FindFirstValue(JwtRegisteredClaimNames.Sub), out int userId))
            return userId;
        return null;
    }
}
