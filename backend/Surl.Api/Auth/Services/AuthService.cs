using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Surl.Api.Common;
using Surl.Api.Data;
using Surl.Api.Users;

namespace Surl.Api.Auth;

public class AuthService(AppDbContext dbContext, IUserService userService, IPasswordHasher<User> passwordHasher,
    ITokenService tokenService, IOptions<JwtOptions> jwtOptionsWrapper, TimeProvider timeProvider,
    ILogger<AuthService> logger) : IAuthService
{
    private readonly AppDbContext _dbContext = dbContext;
    private readonly IUserService _userService = userService;
    private readonly IPasswordHasher<User> _passwordHasher = passwordHasher;
    private readonly ITokenService _tokenService = tokenService;
    private readonly JwtOptions _jwtOptions = jwtOptionsWrapper.Value;
    private readonly TimeProvider _timeProvider = timeProvider;
    private readonly ILogger<AuthService> _logger = logger;

    public async Task<Result<User, AuthError>> RegisterAsync(
        RegisterInput registerInput, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Processing user registration");

        // rely on user service to create a user
        var createUserRequest = new CreateUserRequest(
            registerInput.Username, registerInput.Password, registerInput.Email, AppRoles.User);

        var result = await _userService.CreateUserAsync(createUserRequest, cancellationToken);
        if (!result.IsSuccessful)
        {
            _logger.LogDebug("User registration rejected: {UserError}", result.Error);
            return Result<User, AuthError>.Failure(AuthError.DuplicateUsername); // the only error as of now
        }

        _logger.LogInformation("User registration succeeded for user {UserId}", result.Value!.Id);
        return Result<User, AuthError>.Success(result.Value);
    }

    public async Task<Result<LoginResult, AuthError>> LoginAsync(
        LoginInput loginInput, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Processing login attempt");

        // find user
        var user = await _userService.GetUserByUsernameAsync(loginInput.Username, cancellationToken);
        if (user is null)
        {
            _logger.LogDebug("Login rejected due to invalid credentials: username not found");
            return Result<LoginResult, AuthError>.Failure(AuthError.InvalidCredentials);
        }

        // validate password
        var verificationResult = _passwordHasher.VerifyHashedPassword(
            user, user.PasswordHash, loginInput.Password);
        if (verificationResult != PasswordVerificationResult.Success)
        {
            _logger.LogDebug("Login rejected due to wrong password for user {UserId}", user.Id);
            return Result<LoginResult, AuthError>.Failure(AuthError.InvalidCredentials);
        }

        // issue token
        var accessToken = _tokenService.GenerateAccessToken(user);
        var refreshToken = _tokenService.GenerateRefreshToken();
        var utcNow = _timeProvider.GetUtcNow();

        var refreshTokenObj = new RefreshToken
        {
            Token = refreshToken,
            UserId = user.Id,
            CreatedAt = utcNow,
            ExpiresAt = utcNow.AddDays(_jwtOptions.RefreshTokenExpireInDays),
        };
        await _dbContext.RefreshTokens.AddAsync(refreshTokenObj, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        
        _logger.LogInformation("Login succeeded for user {UserId}", user.Id);
        return Result<LoginResult, AuthError>.Success(new LoginResult(accessToken, refreshTokenObj));
    }

    public async Task<Result<RefreshResult, AuthError>> RefreshAsync(
        string refreshTokenInput, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Processing refresh token rotation");

        // load and validate refresh token
        var refreshToken = await _dbContext.RefreshTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Token == refreshTokenInput, cancellationToken);

        if (refreshToken is null)
        {
            _logger.LogDebug("Refresh token rotation rejected: token not found");
            return Result<RefreshResult, AuthError>.Failure(AuthError.RefreshTokenNotFound);
        }

        if (!refreshToken.IsValidAt(_timeProvider.GetUtcNow()))
        {
            _logger.LogDebug("Refresh token rotation rejected for user {UserId}: token is invalid",
                refreshToken.UserId);
            return Result<RefreshResult, AuthError>.Failure(AuthError.InvalidRefreshToken);
        }
        
        // issue new tokens and deactivate the old one.
        var newAccessToken = _tokenService.GenerateAccessToken(refreshToken.User);
        var newRefreshToken = _tokenService.GenerateRefreshToken();
        var utcNow = _timeProvider.GetUtcNow();

        var newRefreshTokenObj = new RefreshToken
        {
            UserId = refreshToken.UserId,
            Token = newRefreshToken,
            CreatedAt = utcNow,
            ExpiresAt = utcNow.AddDays(_jwtOptions.RefreshTokenExpireInDays),
        };
        await _dbContext.RefreshTokens.AddAsync(newRefreshTokenObj, cancellationToken);
        refreshToken.IsUsed = true;
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Refresh token rotated successfully for user {UserId}", refreshToken.UserId);
        return Result<RefreshResult, AuthError>.Success(new RefreshResult(newAccessToken, newRefreshTokenObj));
    }

    public async Task<Result<RefreshToken, AuthError>> LogoutAsync(string refreshTokenInput, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Logging out");

        // load refresh token
        var refreshToken = await _dbContext.RefreshTokens
            .FirstOrDefaultAsync(t => t.Token == refreshTokenInput, cancellationToken);
        
        if (refreshToken is null)
        {
            _logger.LogDebug("Logging out failed: refresh token not found");
            return Result<RefreshToken, AuthError>.Failure(AuthError.RefreshTokenNotFound);
        }

        if (!refreshToken.IsValidAt(_timeProvider.GetUtcNow()))
        {
            _logger.LogDebug("Logging out failed: refresh token already invalidated");
            return Result<RefreshToken, AuthError>.Failure(AuthError.InvalidRefreshToken);
        }
        // revoke
        refreshToken.IsRevoked = true;
        await _dbContext.SaveChangesAsync();
        
        _logger.LogInformation("Logged out successfully for user {UserId}", refreshToken.UserId);
        return Result<RefreshToken, AuthError>.Success(refreshToken);
    }

    public async Task<Result<User, AuthError>> ChangePasswordAsync(
        int userId, ChangePasswordRequest changePasswordRequest, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Processing password change for user {UserId}", userId);

        // find user
        var user = await _userService.GetUserByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            _logger.LogDebug("Password change rejected because user {UserId} was not found", userId);
            return Result<User, AuthError>.Failure(AuthError.UserNotFound);
        }
        
        // validate old password
        var verificationResult = _passwordHasher.VerifyHashedPassword(
            user, user.PasswordHash, changePasswordRequest.OldPassword);
        if (verificationResult != PasswordVerificationResult.Success)
        {
            _logger.LogDebug("Password change rejected due to wrong old password for user {UserId}", userId);
            return Result<User, AuthError>.Failure(AuthError.InvalidCredentials);
        }

        // update password and invoke existing refresh tokens
        user.PasswordHash = _passwordHasher.HashPassword(user, changePasswordRequest.NewPassword);
        await _dbContext.RefreshTokens
            .Where(t => t.UserId == userId && !t.IsRevoked)
            .ForEachAsync(t => t.IsRevoked = true, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Password changed and refresh tokens revoked for user {UserId}", userId);
        return Result<User, AuthError>.Success(user);
    }
}
