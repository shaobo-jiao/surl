using System.ComponentModel.DataAnnotations;

namespace Surl.Api.Auth;

public class JwtOptions
{
    [Required]
    public string SecretKey { get; set; } = string.Empty;

    [Required]
    public string Issuer { get; init; } = string.Empty;

    [Required]
    public string Audience { get; init; } = string.Empty;
    
    public int AccessTokenExpireInMinutes { get; init; } 

    public int RefreshTokenExpireInDays { get; init; }
}
