using Microsoft.AspNetCore.Mvc;

namespace Surl.Api.IntegrationTests.Helpers;

/// <summary>
/// Helper class to assert problem details from API.
/// </summary>
public class AssertHelper
{
    public static void AssertProblemDetails(ProblemDetails expected, ProblemDetails? actual)
    {
        Assert.NotNull(actual);
        
        Assert.Equal(expected.Status, actual.Status);
        Assert.Equal(expected.Detail, actual.Detail);
        Assert.Equal(expected.Title, actual.Title);

        Assert.Contains("code", actual.Extensions.Keys);
        Assert.Equal(expected.Extensions["code"], actual.Extensions["code"]?.ToString());
    }
}
