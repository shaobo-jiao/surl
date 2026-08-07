using System.Security.Principal;
using Microsoft.AspNetCore.Mvc;
using Surl.Api.Common;

namespace Surl.Api.ShortUrls;

public static class ShortUrlErrorExtensions
{
    /// <summary>
    /// Maps ShortUrlError to ProblemDetails for endpoint return use.
    /// </summary>
    /// <param name="error"></param>
    /// <returns></returns>
    public static ProblemDetails ToProblemDetails(this ShortUrlError error)
    {
        return error switch
        {
            ShortUrlError.NotFound => ProblemDetailsHelper.Create(
                status: StatusCodes.Status404NotFound, 
                code: "short_url_not_found",
                title: "Short Url not found"),
            
            ShortUrlError.Expired => ProblemDetailsHelper.Create(
                status: StatusCodes.Status410Gone, 
                code: "short_url_expired",
                title: "Short Url expired"),
            
            ShortUrlError.InvalidCode => ProblemDetailsHelper.Create(
                status: StatusCodes.Status404NotFound, 
                code: "short_url_invalid",
                title: "Invalid short Url"),
            
            _ => ProblemDetailsHelper.Create(
                status: StatusCodes.Status400BadRequest,
                code: "short_url_general_error"
            )
        };
    }
}
