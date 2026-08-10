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
                title: "Short URL not found",
                detail: "The short URL is not found."),
            
            ShortUrlError.Expired => ProblemDetailsHelper.Create(
                status: StatusCodes.Status410Gone, 
                code: "short_url_expired",
                title: "Short URL expired",
                detail: "The short URL has already expired."),
            
            ShortUrlError.InvalidCode => ProblemDetailsHelper.Create(
                status: StatusCodes.Status404NotFound, 
                code: "short_url_invalid",
                title: "Invalid short URL", 
                detail: "The short URL format is invalid."),
            
            _ => ProblemDetailsHelper.Create(
                status: StatusCodes.Status400BadRequest,
                code: "short_url_general_error",
                title: "Short URL General Error",
                detail: "There is something wrong with the short URL."
            )
        };
    }
}
