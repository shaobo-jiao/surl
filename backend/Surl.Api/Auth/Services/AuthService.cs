using Surl.Api.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Surl.Api.Data;

namespace Surl.Api.Auth;

public class AuthService(AppDbContext dbContext, IUserService userService, IPasswordHasher<User> passwordHasher,
    ITokenService tokenService, IOptions<JwtOptions> jwtOptionsWrapper, TimeProvider timeProvider) : IAuthService
{
    private AppDbContext _dbContext = dbContext;
    private IUserService _userService = userService;
    private IPasswordHasher<User> _passwordHasher = passwordHasher;
    private ITokenService _tokenService = tokenService;
    private JwtOptions _jwtOptions = jwtOptionsWrapper.Value;
    private TimeProvider _timeProvider = timeProvider; 

    public Task<User> RegisterAsync(RegisterRequest registerRequest)
    {
        // rely on user service to create a user
        CreateUserRequest createUserRequest = new(registerRequest.Username, registerRequest.Password, registerRequest.Email, AppRoles.User);
        return _userService.CreateUserAsync(createUserRequest);
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest loginRequest)
    {
        // find user, validate password, issue token; 
        var user = await _userService.GetUserByUsernameAsync(loginRequest.Username) 
            ?? throw new UserNotFoundException();

        var verificationResult = _passwordHasher.VerifyHashedPassword(
            user, user.PasswordHash, loginRequest.Password);
        if (verificationResult != PasswordVerificationResult.Success) 
            throw new InvalidCredentialsException();

        string accessToken = _tokenService.GenerateAccessToken(user);
        string refreshToken = _tokenService.GenerateRefreshToken();

        // save refresh token into db;
        var utcNow = _timeProvider.GetUtcNow();
        await _dbContext.RefreshTokens.AddAsync(new RefreshToken()
        {
            Token = refreshToken,
            UserId = user.Id,
            CreatedAt = utcNow,
            ExpiresAt = utcNow.AddDays(_jwtOptions.RefreshTokenExpireInDays),
        });
        await _dbContext.SaveChangesAsync();

        return new LoginResponse(accessToken, refreshToken);
    }

    public async Task<RefreshResponse> RefreshAsync(RefreshRequest refreshRequest)
    {
        // load and validate refresh token;
        var refreshToken = await _dbContext.RefreshTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Token == refreshRequest.RefreshToken);
        
        if (refreshToken == null) 
            throw new InvalidRefreshTokenException("Refresh token not found");
        else if (!refreshToken.IsValidAt(_timeProvider.GetUtcNow()))
            throw new InvalidRefreshTokenException("The refresh token is either used, revoked or expired already.");
        
        // consume refresh token
        string newAccessToken = _tokenService.GenerateAccessToken(refreshToken.User);
        string newRefreshToken = _tokenService.GenerateRefreshToken();
        var utcNow = _timeProvider.GetUtcNow();
        await _dbContext.RefreshTokens.AddAsync(new RefreshToken()
        {
            UserId = refreshToken.UserId,
            Token = newRefreshToken,
            CreatedAt = utcNow,
            ExpiresAt = utcNow.AddDays(_jwtOptions.RefreshTokenExpireInDays),
        });
        refreshToken.IsUsed = true;
        await _dbContext.SaveChangesAsync();

        return new RefreshResponse(newAccessToken, newRefreshToken);
    }

    public async Task ChangePasswordAsync(int userId, ChangePasswordRequest changePasswordRequest)
    {
        // load user;
        var user = await _userService.GetUserByIdAsync(userId) 
            ?? throw new UserNotFoundException();

        // validate password;
        var verificationResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, changePasswordRequest.OldPassword);
        if (verificationResult != PasswordVerificationResult.Success) 
            throw new InvalidCredentialsException();

        // update password and revoke existing refresh tokens;
        user.PasswordHash = _passwordHasher.HashPassword(user, changePasswordRequest.NewPassword);
        _dbContext.RefreshTokens
            .Where(t => t.UserId == userId && !t.IsRevoked)
            .ToList()
            .ForEach(t => t.IsRevoked = true);
        await _dbContext.SaveChangesAsync();
    }
}
