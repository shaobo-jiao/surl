using Surl.Api.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Surl.Api.Data;

namespace Surl.Api.Auth;

public class AuthService : IAuthService
{
    private AppDbContext _dbContext;
    private IUserService _userService;
    private IPasswordHasher<User> _passwordHasher;
    private ITokenService _tokenService;
    private JwtOptions _jwtOptions;

    public AuthService(AppDbContext dbContext, IUserService userService, IPasswordHasher<User> passwordHasher,
        ITokenService tokenService, IOptions<JwtOptions> jwtOptions)
    {
        _dbContext = dbContext;
        _userService = userService;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _jwtOptions = jwtOptions.Value;
    }

    public Task<User> RegisterAsync(RegisterRequest registerRequest)
    {
        CreateUserRequest createUserRequest = new(registerRequest.Username, registerRequest.Password, registerRequest.Email, Roles.User);
        return _userService.CreateUserAsync(createUserRequest);
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest loginRequest)
    {
        // find user, validate password, issue token; 
        var user = await _userService.GetUserByUsernameAsync(loginRequest.Username) ?? throw new UserNotFoundException();

        var verificationResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, loginRequest.Password);
        if (verificationResult != PasswordVerificationResult.Success) throw new InvalidCredentialsException();

        string accessToken = _tokenService.GenerateAccessToken(user);
        string refreshToken = _tokenService.GenerateRefreshToken();

        // save refresh token into db;
        await _dbContext.RefreshTokens.AddAsync(new RefreshToken()
        {
            Token = refreshToken,
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenExpireInDays),
        });
        await _dbContext.SaveChangesAsync();

        return new LoginResponse(accessToken, refreshToken);
    }

    public async Task<RefreshResponse> RefreshAsync(RefreshRequest refreshRequest)
    {
        // load and validate refresh token; general error validation here; later can split and throw as exceptions;
        var storedRefreshToken = await _dbContext.RefreshTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Token == refreshRequest.RefreshToken);
        
        if (storedRefreshToken == null) throw new InvalidRefreshTokenException("Refresh token not found");
        else if (storedRefreshToken.IsUsed || storedRefreshToken.IsRevoked) throw new InvalidRefreshTokenException("Refresh token already used or revoked");
        else if (storedRefreshToken.ExpiresAt < DateTime.UtcNow) throw new InvalidRefreshTokenException("Refresh token expired");
        
        // consume refresh token
        string newAccessToken = _tokenService.GenerateAccessToken(storedRefreshToken.User);
        string newRefreshToken = _tokenService.GenerateRefreshToken();
        await _dbContext.RefreshTokens.AddAsync(new RefreshToken()
        {
            UserId = storedRefreshToken.UserId,
            Token = newRefreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenExpireInDays),
        });
        storedRefreshToken.IsUsed = true;
        await _dbContext.SaveChangesAsync();

        return new RefreshResponse(newAccessToken, newRefreshToken);
    }

    public async Task ChangePasswordAsync(int userId, ChangePasswordRequest changePasswordRequest)
    {
        // load user;
        var user = await _userService.GetUserByIdAsync(userId) ?? throw new UserNotFoundException();

        // validate password;
        var verificationResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, changePasswordRequest.OldPassword);
        if (verificationResult != PasswordVerificationResult.Success) throw new InvalidCredentialsException();

        // update password and revoke existing refresh tokens;
        user.PasswordHash = _passwordHasher.HashPassword(user, changePasswordRequest.NewPassword);
        _dbContext.RefreshTokens
            .Where(t => t.UserId == userId && !t.IsRevoked)
            .ToList()
            .ForEach(t => t.IsRevoked = true);
        await _dbContext.SaveChangesAsync();
    }
}
