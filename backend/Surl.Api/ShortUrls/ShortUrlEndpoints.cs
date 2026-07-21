using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Surl.Api.Common;

namespace Surl.Api.ShortUrls;

public static class ShortUrlEndpoints
{
    public static void MapShortUrlEndpoints(this IEndpointRouteBuilder app)
    {
        var shortUrlApi = app.MapGroup("/short-urls");
        
        shortUrlApi.MapPost("/", CreateShortUrlAsync)
            .WithValidationFilter<CreateShortUrlRequest>();
        
        shortUrlApi.MapGet("/{code}", GetOriginalUrlAsync)
            .WithName("GetOriginalUrl")
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status410Gone)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        app.MapGet("/redirection/{code}", GetOriginalUrlForRedirectionAsync)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status410Gone)
            .ProducesProblem(StatusCodes.Status400BadRequest);
    }

    /// <summary>
    /// Creates a new Short URL that maps to the given original URL.
    /// </summary>
    /// <param name="apiSettings"></param>
    /// <param name="shortUrlSvc"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    public static async Task<Results<CreatedAtRoute<ShortUrlResponse>, BadRequest>> CreateShortUrlAsync(
        ShortUrlService shortUrlSvc, CreateShortUrlRequest request, CancellationToken cancellationToken)
    {
        var shortUrl = await shortUrlSvc.CreateShortUrlAsync(request, cancellationToken);
        var response = shortUrl.ToResponse();
        return TypedResults.CreatedAtRoute(response, "GetOriginalUrl", new { code = shortUrl.Code });
    }

    /// <summary>
    /// Gets original URL mapping information for the given short URL
    /// </summary>
    /// <param name="apiSettings"></param>
    /// <param name="shortUrlSvc"></param>
    /// <param name="code"></param>
    /// <returns></returns>
    public static async Task<Results<Ok<ShortUrlResponse>, ProblemHttpResult>> GetOriginalUrlAsync(
        ShortUrlService shortUrlSvc, [FromRoute] string code, CancellationToken cancellationToken)
    {
        try
        {
            var shortUrl = await shortUrlSvc.GetShortUrlByCodeAsync(code, cancellationToken);
            return TypedResults.Ok(shortUrl.ToResponse());
        }
        catch (ShortUrlBaseException ex)
        {
            return ex.ToProblem();
        }
    }

    /// <summary>
    /// Gets original URL for redirection and triggers click event saving.
    /// </summary>
    public static async Task<Results<Ok<string>, ProblemHttpResult>> GetOriginalUrlForRedirectionAsync(
        ShortUrlService shortUrlSvc, ShortUrlClickEventQueue clickEventQueue, [FromRoute] string code, 
        CancellationToken cancellationToken)
    {
        try
        {
            var shortUrl = await shortUrlSvc.GetShortUrlByCodeAsync(code, cancellationToken);
            var clickEvent = new ShortUrlClickEvent(shortUrl.Id, shortUrl.Code, DateTimeOffset.UtcNow);
            await clickEventQueue.EnqueueAsync(clickEvent, cancellationToken);

            return TypedResults.Ok(shortUrl.OriginalUrl);
        }
        catch (ShortUrlBaseException ex)
        {
            return ex.ToProblem();
        }
    }
}
