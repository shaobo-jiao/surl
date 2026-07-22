using System.Security.Claims;
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

        shortUrlApi.MapGet("/", GetShortUrlsAsync)
            .WithValidationFilter<QueryShortUrlParams>()
            .RequireAuthorization();

        shortUrlApi.MapGet("/{id:long}", GetShortUrlByIdAsync)
            .WithName("GetShortUrlById")
            .RequireAuthorization();
        
        shortUrlApi.MapPatch("/{id:long}", UpdateShortUrlAsync)
            .WithValidationFilter<UpdateShortUrlRequest>()
            .RequireAuthorization();
        
        shortUrlApi.MapDelete("/{id:long}", DeleteShortUrlAsync)
            .RequireAuthorization();

        app.MapGet("/redirection/{code}", GetRedirectionUrlAsync)
            .ProducesProblem(StatusCodes.Status410Gone);
    }

    public static async Task<Results<CreatedAtRoute<ShortUrlResponse>, BadRequest>> CreateShortUrlAsync(
        IShortUrlService shortUrlSvc, CreateShortUrlRequest request, ClaimsPrincipal principal, CancellationToken cancellationToken)
    {
        var userId = principal.GetUserId();
        var shortUrl = await shortUrlSvc.CreateShortUrlAsync(request, userId, cancellationToken);
        var response = shortUrl.ToResponse();
        return TypedResults.CreatedAtRoute(response, "GetShortUrlById", new { id = shortUrl.Id });
    }

    public static async Task<Results<Ok<ShortUrlResponse>, NotFound>> GetShortUrlByIdAsync(
        IShortUrlService shortUrlService, [FromRoute] long id, ClaimsPrincipal principal, CancellationToken cancellationToken)
    {
        var userId = principal.GetUserId();
        var shortUrl = await shortUrlService.GetShortUrlByIdAsync(id, userId, cancellationToken);
        if (shortUrl is null) return TypedResults.NotFound();
        return TypedResults.Ok(shortUrl.ToResponse());
    }

    public static async Task<Ok<PagedResult<ShortUrlResponse>>> GetShortUrlsAsync(
        IShortUrlService shortUrlService, [AsParameters] QueryShortUrlParams queryParams, ClaimsPrincipal principal, CancellationToken cancellationToken)
    {
        var userId = principal.GetUserId();
        PagedResult<ShortUrl> shortUrls = await shortUrlService.GetShortUrlsAsync(queryParams, userId, cancellationToken);
        return TypedResults.Ok(shortUrls.Map(u => u.ToResponse()));
    }

    /// <summary>
    /// Gets the original URL for redirection from given code, and triggers a click event.
    /// </summary>
    /// <returns>The original URL</returns>
    public static async Task<Results<Ok<string>, NotFound, BadRequest, ProblemHttpResult>> GetRedirectionUrlAsync(
        IShortUrlService shortUrlService, [FromRoute] string code, ShortUrlClickEventQueue clickEventQueue, CancellationToken cancellationToken)
    {
        var result = await shortUrlService.GetRedirectionUrlAsync(code, cancellationToken);
        if (result.IsSuccessful)
        {
            var shortUrl = result.Value!;
            var clickEvent = new ShortUrlClickEvent(shortUrl.Id, shortUrl.Code, DateTimeOffset.UtcNow);
            await clickEventQueue.EnqueueAsync(clickEvent, cancellationToken);
            return TypedResults.Ok(shortUrl.OriginalUrl);
        }
        else
        {
            // failure
            return result.Error switch
            {
                ShortUrlError.NotFound => TypedResults.NotFound(),

                ShortUrlError.InvalidCode => TypedResults.Problem(
                    statusCode: StatusCodes.Status400BadRequest, 
                    title: "Invalid URL",
                    detail: "The provided short url is invalid."),

                ShortUrlError.Expired => TypedResults.Problem(
                    statusCode: StatusCodes.Status410Gone,
                    title: "Short URL Expired",
                    detail: "The short URL has expired"),

                _ => TypedResults.BadRequest(),
            };
        }
    }

    public static async Task<Results<Ok<ShortUrlResponse>, NotFound>> UpdateShortUrlAsync(
        IShortUrlService shortUrlService, UpdateShortUrlRequest request, [FromRoute] long id, 
        ClaimsPrincipal principal, CancellationToken cancellationToken)
    {
        var userId = principal.GetUserId();
        var result = await shortUrlService.UpdateShortUrlAsync(id, request, userId, cancellationToken);
        if (result.IsSuccessful)
            return TypedResults.Ok(result.Value!.ToResponse());
        else
            return TypedResults.NotFound(); // only ShortUrlError.NotFound here
    }

    public static async Task<Results<NoContent, NotFound>> DeleteShortUrlAsync(
        IShortUrlService shortUrlService, [FromRoute] long id, ClaimsPrincipal principal, CancellationToken cancellationToken)
    {
        var userId = principal.GetUserId();
        var result = await shortUrlService.DeleteShortUrlAsync(id, userId, cancellationToken);
        return result.IsSuccessful ? TypedResults.NoContent() : TypedResults.NotFound(); // only ShortUrlError.NotFound here
    }
}
