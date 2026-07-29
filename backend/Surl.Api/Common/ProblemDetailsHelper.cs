using Microsoft.AspNetCore.Mvc;

namespace Surl.Api.Common;

public static class ProblemDetailsHelper
{
    /// <summary>
    /// Creates a ProblemDetails instance that helps map service layer Errors to Endpoint responses. 
    /// </summary>
    /// <param name="status"></param>
    /// <param name="code"></param>
    /// <param name="title"></param>
    /// <param name="detail"></param>
    /// <returns></returns>
    public static ProblemDetails Create(int status, string code, string? title = null, string? detail = null)
    {
        return new ProblemDetails()
        {
            Status = status,
            Title = title,
            Detail = detail,
            Extensions = new Dictionary<string, object?>()
            {
                ["code"] = code,
            },
        }; 
    }
}
