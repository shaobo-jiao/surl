using Microsoft.AspNetCore.Http.HttpResults;

namespace Surl.Api.ShortUrls;

public static class ShortUrlExceptionResponseExtensions
{
    public static ProblemHttpResult ToProblem(this ShortUrlBaseException ex)
    {
        return TypedResults.Problem(
            title: "Short URL Error",
            detail: ex.Message,
            statusCode: ex switch
            {
                ShortUrlNotFoundException => StatusCodes.Status404NotFound,
                ShortUrlExpiredException => StatusCodes.Status410Gone,
                _ => StatusCodes.Status400BadRequest
            }
        );
    }
}
