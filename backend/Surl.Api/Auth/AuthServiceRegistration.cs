using System.IdentityModel.Tokens.Jwt;
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
            .BindConfiguration("JwtOptions")
            .PostConfigure<IConfiguration>((options, configuration) =>
            {
                // overwrite jwt secret from the secret file
                var jwtSecretKeyFile = configuration.GetValue<string>("Secrets:JWT_SECRET_KEY_FILE");
                if (File.Exists(jwtSecretKeyFile))
                {
                    options.SecretKey = File.ReadAllText(jwtSecretKeyFile).TrimEnd();
                }
            })
            .ValidateDataAnnotations()
            .ValidateOnStart();

        // JWT bearer authentication
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme);

        // options for JWT bearer authentication
        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<JwtOptions>, TimeProvider>((options, jwtOptionsWrapper, timeProvider) =>
            {
                var jwtOptions = jwtOptionsWrapper.Value;
                options.TimeProvider = timeProvider;
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
                    
                    NameClaimType = JwtRegisteredClaimNames.Name,
                    RoleClaimType = AppClaimTypes.Role,
                };
                options.MapInboundClaims = false;
            });
        services.AddAuthorization();

        // reusable field validators
        services.AddScoped<UsernameValidator>();
        services.AddScoped<PasswordValidator>();

        // request validators
        services.AddScoped<IValidator<RegisterRequest>, RegisterRequestValidator>();
        services.AddScoped<IValidator<LoginRequest>, LoginRequestValidator>();
        services.AddScoped<IValidator<RefreshRequest>, RefreshRequestValidator>();
        services.AddScoped<IValidator<ChangePasswordRequest>, ChangePasswordRequestValidator>();
    }
}
