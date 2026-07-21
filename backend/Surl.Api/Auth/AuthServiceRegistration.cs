using System.Security.Claims;
using System.Text;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Surl.Api.Auth;

public static class AuthServiceRegistration
{
    /// <summary>
    /// Registers all services required by the Auth module
    /// </summary>
    /// <param name="services"></param>
    public static void AddAuthServices(this IServiceCollection services)
    {
        // module services
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ITokenService, JwtService>();

        // JWT options
        services.AddOptions<JwtOptions>()
            .BindConfiguration("Jwt")
            .PostConfigure<IConfiguration>((options, configuration) =>
            {
                // overwrite jwt secret from the secret file
                var jwtSecretFile = configuration.GetValue<string>("Secrets:JWT_SECRET_FILE");
                if (File.Exists(jwtSecretFile))
                {
                    options.SecretKey = File.ReadAllText(jwtSecretFile).TrimEnd();
                }
            })
            .ValidateDataAnnotations()
            .ValidateOnStart();

        // authentication
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme);

        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<JwtOptions>>((options, jwtOptionsWrapper) =>
            {
                var jwtOptions = jwtOptionsWrapper.Value;

                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SecretKey)),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,
                    ValidateIssuer = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwtOptions.Audience,
                    NameClaimType = ClaimTypes.Name,
                    RoleClaimType = ClaimTypes.Role,
                };
                // opts.MapInboundClaims = false;
            });
        services.AddAuthorization();

        // validators
        services.AddScoped<IValidator<RegisterRequest>, RegisterRequestValidator>();
        services.AddScoped<IValidator<LoginRequest>, LoginRequestValidator>();
        services.AddScoped<IValidator<RefreshRequest>, RefreshRequestValidator>();
        services.AddScoped<IValidator<ChangePasswordRequest>, ChangePasswordRequestValidator>();
    }
}
