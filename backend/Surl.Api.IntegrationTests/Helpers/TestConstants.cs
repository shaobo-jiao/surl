namespace Surl.Api.IntegrationTests.Helpers;

public class TestConstants
{
    /// <summary>
    /// Default test start time, used by the fake time provider. 
    /// 
    /// This is set to be near real date time rather a fixed past date time 
    /// because some components doesn't inject TimeProvider.
    /// e.g. HttpClient checks real system datetime to drop cookies. Setting a 
    /// fixed date datetime produces refresh tokens considered expired by HttpClient. 
    /// 
    /// The time part is truncated because Postgres timestamptz resolution is microseconds, 
    /// while DateTimeOffset's is smaller ticks. Comparison of DateTimeOffset values after saving 
    /// may differ and fails some asserts.
    /// </summary>
    public static readonly DateTimeOffset TestStartTime = new(DateTimeOffset.UtcNow.Date, TimeSpan.Zero);

    /// <summary>
    /// Base address of the test server. 
    /// https is used so that refresh token cookie (secure = true) can be sent.
    /// </summary>
    public const string TestBaseAddress = "https://localhost";

    public const string TestUser1 = "test-user-1";
    public const string TestUser2 = "test-user-2";
    public const string TestPassword = "P@ssw0rd_123";
}
