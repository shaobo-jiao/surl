using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Surl.Api.IntegrationTests.Helpers;

/// <summary>
/// Implements a custom token lifetime validator based on TimeProvider for testing. 
/// 
/// This is needed because default JwtBearerOptions uses TimeProvider.System for LifeTime validation, 
/// despite injecting own provider to JwtBearerOptions.TimeProvider.
/// 
/// Reference: 
/// - https://stackoverflow.com/questions/79600635/how-to-setup-jwt-authentication-with-timeprovider-in-net-9-integration-tests
/// - https://github.com/AzureAD/azure-activedirectory-identitymodel-extensions-for-dotnet/blob/dev/src/Microsoft.IdentityModel.Tokens/Experimental/Validation/Validators.Lifetime.cs
/// </summary>
public class CustomJwtBearerOptionsLifeTimeValidator(TimeProvider timeProvider, 
    ILogger<CustomJwtBearerOptionsLifeTimeValidator> logger) : IPostConfigureOptions<JwtBearerOptions>
{
    /// <summary>
    /// injected TimeProvider, which can be FakeTimeProvider for testing. 
    /// </summary>
    private readonly TimeProvider _timeProvider = timeProvider;

    private readonly ILogger<CustomJwtBearerOptionsLifeTimeValidator> _logger = logger;

    /// <summary>
    /// Post configures LifetimeValidator after existing configurations. 
    /// </summary>
    /// <param name="name"></param>
    /// <param name="options"></param>
    public void PostConfigure(string? name, JwtBearerOptions options)
    {
        options.TokenValidationParameters.LifetimeValidator = (notBefore, expires, securityToken, validationParameters) =>
        {
            if (!expires.HasValue)
                return false;

            if (notBefore.HasValue && notBefore.Value > expires)
                return false;

            var utcNow = _timeProvider.GetUtcNow().UtcDateTime;
            if (notBefore.HasValue && (notBefore.Value > DateTimeUtil.Add(utcNow, validationParameters.ClockSkew)))
            {
                _logger.LogInformation("The token is invalid before {NotBefore}. Current time is {UtcNow} with skew {Skew}", 
                    notBefore, utcNow, validationParameters.ClockSkew);                
                return false;
            }
            if (expires.HasValue && (expires.Value < DateTimeUtil.Add(utcNow, validationParameters.ClockSkew.Negate())))
            {
                _logger.LogInformation("The token is invalid after {Expires}. Current time is {UtcNow} with skew {Skew}", 
                    expires, utcNow, validationParameters.ClockSkew);
                return false;
            }
            // valid token
            return true;
        };
    }
}
