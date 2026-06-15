using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace UrlShortener.Api;

public static class ShortUrlEndpoints
{
    public static void MapShortUrlEndpoints(this IEndpointRouteBuilder app)
    {
        var shortUrlApi = app.MapGroup("/api/short-urls");
        shortUrlApi.MapPost("/", CreateShortUrlAsync);
        
        shortUrlApi.MapGet("/{code}", GetOriginalUrlAsync)
            .WithName("GetOriginalUrl")
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status410Gone)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        app.MapGet("/{code}", RedirectToOriginalUrl)
            .ExcludeFromDescription(); // exclude from Swagger
    }

    /// <summary>
    /// Creates a new Short URL that maps to the given original URL.
    /// </summary>
    /// <param name="apiSettings"></param>
    /// <param name="shortUrlSvc"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    public static async Task<Results<CreatedAtRoute<ShortUrlResponse>, BadRequest>> CreateShortUrlAsync(
        IOptions<ApiSettings> apiSettings, ShortUrlService shortUrlSvc, CreateShortUrlRequest request, 
        CancellationToken cancellationToken)
    {
        var shortUrl = await shortUrlSvc.CreateShortUrlAsync(request, cancellationToken);
        var response = shortUrl.ToResponse(apiSettings.Value);
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
        IOptions<ApiSettings> apiSettings, ShortUrlService shortUrlSvc, [FromRoute] string code, 
        CancellationToken cancellationToken)
    {
        try
        {
            var shortUrl = await shortUrlSvc.GetShortUrlByCodeAsync(code, cancellationToken);
            return TypedResults.Ok(shortUrl.ToResponse(apiSettings.Value));
        }
        catch (ShortUrlBaseException ex)
        {
            return ex.ToProblem();
        }
    }

    /// <summary>
    /// Redirects to the original URL mapped by the given short URL, and logs the click event of the found URL entity.
    /// </summary>
    public static async Task<Results<RedirectHttpResult, ProblemHttpResult>> RedirectToOriginalUrl(
        ShortUrlService shortUrlSvc, ShortUrlClickEventQueue _clickEventQueue, [FromRoute] string code, 
        CancellationToken cancellationToken)
    {
        try
        {
            var shortUrl = await shortUrlSvc.GetShortUrlByCodeAsync(code, cancellationToken);
            var clickEvent = new ShortUrlClickEvent(shortUrl.Id, shortUrl.Code, DateTimeOffset.UtcNow);
            await _clickEventQueue.EnqueueAsync(clickEvent, cancellationToken);

            return TypedResults.Redirect(shortUrl.OriginalUrl, permanent: false);
        }
        catch (ShortUrlBaseException ex)
        {
            return ex.ToProblem();
        }
    }
}
