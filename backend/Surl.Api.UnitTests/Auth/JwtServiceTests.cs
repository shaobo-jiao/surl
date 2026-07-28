using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;
using Microsoft.IdentityModel.JsonWebTokens;
using Surl.Api.Auth;
using Surl.Api.UnitTests.Configuration;
using Surl.Api.Users;

namespace Surl.Api.UnitTests.Auth;

public class JwtServiceTests
{
    private readonly JwtOptions _jwtOptions;
    private readonly FakeTimeProvider _timeProvider;
    private readonly JwtService _jwtService;

    public JwtServiceTests()
    {
        _jwtOptions = new JwtOptions()
        {
            SecretKey = "tests.jwt.secretkey.some.extra.string.to.make.it.long.enough",
            Issuer = "tests.jwt.issuer",
            Audience = "tests.jwt.audience",
            AccessTokenExpireInMinutes = 15,
            RefreshTokenExpireInDays = 7,
        };
        _timeProvider = new FakeTimeProvider(TestConstants.TestStartTime);
        _jwtService = new JwtService(Options.Create(_jwtOptions), _timeProvider);
    }

    [Fact]
    public void GenerateAccessToken_ShouldReturnValidJwt()
    {
        // Arrange: create a user
        var utcNow = _timeProvider.GetUtcNow();
        var user = new User("test.jwt.user")
        {
            CreatedAt = utcNow,
            Role = AppRoles.User,
        };
        typeof(User).GetProperty(nameof(User.Id))?.SetValue(user, 1);

        // Act: generate token
        var token = _jwtService.GenerateAccessToken(user);
        var jwt = new JsonWebTokenHandler().ReadJsonWebToken(token);

        // Arrange: validate token fields
        Assert.NotEmpty(token);
        Assert.Equal(_jwtOptions.Issuer, jwt.Issuer);
        Assert.Equal(_jwtOptions.Audience, jwt.Audiences.First());
        
        var expectedExpiriesAt = utcNow.AddMinutes(_jwtOptions.AccessTokenExpireInMinutes).UtcDateTime;
        Assert.Equal(expectedExpiriesAt, jwt.ValidTo);

        Assert.Equal(user.Id.ToString(), jwt.GetClaim(JwtRegisteredClaimNames.Sub).Value);
        Assert.Equal(user.Username, jwt.GetClaim(JwtRegisteredClaimNames.Name).Value);
        Assert.Equal(user.Role, jwt.GetClaim(AppClaimTypes.Role).Value);
    }

    [Fact]
    public void GenerateRefreshToken_ShouldReturnRefreshToken()
    {
        // Act
        var token = _jwtService.GenerateRefreshToken();

        // Assert
        Assert.NotEmpty(token);
        Assert.Equal(44, token.Length);
    }
}
