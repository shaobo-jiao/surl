using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Surl.Api.Common;
using Surl.Api.Data;
using Surl.Api.Users;

namespace Surl.Api.Auth;

public class AuthService(AppDbContext dbContext, IUserService userService, IPasswordHasher<User> passwordHasher,
    ITokenService tokenService, IOptions<JwtOptions> jwtOptionsWrapper, TimeProvider timeProvider) : IAuthService
{
    private readonly AppDbContext _dbContext = dbContext;
    private readonly IUserService _userService = userService;
    private readonly IPasswordHasher<User> _passwordHasher = passwordHasher;
    private readonly ITokenService _tokenService = tokenService;
    private readonly JwtOptions _jwtOptions = jwtOptionsWrapper.Value;
    private readonly TimeProvider _timeProvider = timeProvider;

    public async Task<Result<User, AuthError>> RegisterAsync(
        RegisterRequest registerRequest, CancellationToken cancellationToken)
    {
        // rely on user service to create a user
        var createUserRequest = new CreateUserRequest(
            registerRequest.Username, registerRequest.Password, registerRequest.Email, AppRoles.User);

        try
        {
            // TODO: later to modify user service
            var user = await _userService.CreateUserAsync(createUserRequest);
            return Result<User, AuthError>.Success(user);
        }
        catch (DuplicateUsernameException)
        {
            return Result<User, AuthError>.Failure(AuthError.DuplicateUsername);
        }
    }

    public async Task<Result<LoginResponse, AuthError>> LoginAsync(
        LoginRequest loginRequest, CancellationToken cancellationToken)
    {
        // find user
        var user = await _userService.GetUserByUsernameAsync(loginRequest.Username);
        if (user is null)
            return Result<LoginResponse, AuthError>.Failure(AuthError.InvalidCredentials);

        // validate password
        var verificationResult = _passwordHasher.VerifyHashedPassword(
            user, user.PasswordHash, loginRequest.Password);
        if (verificationResult != PasswordVerificationResult.Success)
            return Result<LoginResponse, AuthError>.Failure(AuthError.InvalidCredentials);

        // issue token
        var accessToken = _tokenService.GenerateAccessToken(user);
        var refreshToken = _tokenService.GenerateRefreshToken();
        var utcNow = _timeProvider.GetUtcNow();

        await _dbContext.RefreshTokens.AddAsync(new RefreshToken
        {
            Token = refreshToken,
            UserId = user.Id,
            CreatedAt = utcNow,
            ExpiresAt = utcNow.AddDays(_jwtOptions.RefreshTokenExpireInDays),
        }, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result<LoginResponse, AuthError>.Success(new LoginResponse(accessToken, refreshToken));
    }

    public async Task<Result<RefreshResponse, AuthError>> RefreshAsync(
        RefreshRequest refreshRequest, CancellationToken cancellationToken)
    {
        // load and validate refresh token
        var refreshToken = await _dbContext.RefreshTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Token == refreshRequest.RefreshToken, cancellationToken);

        if (refreshToken is null || !refreshToken.IsValidAt(_timeProvider.GetUtcNow()))
            return Result<RefreshResponse, AuthError>.Failure(AuthError.InvalidRefreshToken);
        
        // issue new tokens and deactivate the old one.
        var newAccessToken = _tokenService.GenerateAccessToken(refreshToken.User);
        var newRefreshToken = _tokenService.GenerateRefreshToken();
        var utcNow = _timeProvider.GetUtcNow();

        await _dbContext.RefreshTokens.AddAsync(new RefreshToken
        {
            UserId = refreshToken.UserId,
            Token = newRefreshToken,
            CreatedAt = utcNow,
            ExpiresAt = utcNow.AddDays(_jwtOptions.RefreshTokenExpireInDays),
        }, cancellationToken);
        refreshToken.IsUsed = true;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result<RefreshResponse, AuthError>.Success(
            new RefreshResponse(newAccessToken, newRefreshToken));
    }

    public async Task<Result<User, AuthError>> ChangePasswordAsync(
        int userId, ChangePasswordRequest changePasswordRequest, CancellationToken cancellationToken)
    {
        // find user
        var user = await _userService.GetUserByIdAsync(userId);
        if (user is null)
            return Result<User, AuthError>.Failure(AuthError.UserNotFound);
        
        // validate old password
        var verificationResult = _passwordHasher.VerifyHashedPassword(
            user, user.PasswordHash, changePasswordRequest.OldPassword);
        if (verificationResult != PasswordVerificationResult.Success)
            return Result<User, AuthError>.Failure(AuthError.InvalidCredentials);

        // update password and invoke existing refresh tokens
        user.PasswordHash = _passwordHasher.HashPassword(user, changePasswordRequest.NewPassword);
        await _dbContext.RefreshTokens
            .Where(t => t.UserId == userId && !t.IsRevoked)
            .ForEachAsync(t => t.IsRevoked = true, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result<User, AuthError>.Success(user);
    }
}
