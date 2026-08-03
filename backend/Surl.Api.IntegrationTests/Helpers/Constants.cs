namespace Surl.Api.IntegrationTests.Helpers;

public class Constants
{
    /// <summary>
    /// default test start time, used by the fake time provider.
    /// </summary>
    public static readonly DateTimeOffset TestStartTime = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    public const string TestUser = "user1";
    public const string TestPassword = "P@ssw0rd_123";
}
