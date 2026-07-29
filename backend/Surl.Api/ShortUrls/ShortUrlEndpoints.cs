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
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAuthorization();
        
        shortUrlApi.MapDelete("/{id:long}", DeleteShortUrlAsync)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAuthorization();

        app.MapGet("/redirection/{code}", GetRedirectionUrlAsync)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status410Gone);
    }

    public static async Task<CreatedAtRoute<ShortUrlResponse>> CreateShortUrlAsync(
        IShortUrlService shortUrlSvc, CreateShortUrlRequest request, ClaimsPrincipal principal, CancellationToken cancellationToken)
    {
        // can create anonymous or user-linked short urls;
        var authenticated = principal.Identity?.IsAuthenticated ?? false;
        int? userId = authenticated ? principal.GetUserId() : null;
        
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
    public static async Task<Results<Ok<string>, ProblemHttpResult>> GetRedirectionUrlAsync(
        IShortUrlService shortUrlService, [FromRoute] string code, ShortUrlClickEventQueue clickEventQueue, 
        TimeProvider timeProvider, CancellationToken cancellationToken)
    {
        var result = await shortUrlService.GetRedirectionUrlAsync(code, cancellationToken);
        if (result.IsSuccessful)
        {
            var shortUrl = result.Value!;
            var clickEvent = new ShortUrlClickEvent(shortUrl.Id, shortUrl.Code, timeProvider.GetUtcNow());
            await clickEventQueue.EnqueueAsync(clickEvent, cancellationToken);
            return TypedResults.Ok(shortUrl.OriginalUrl);
        }
        else 
            return TypedResults.Problem(result.Error.ToProblemDetails());
    }

    public static async Task<Results<Ok<ShortUrlResponse>, ProblemHttpResult>> UpdateShortUrlAsync(
        IShortUrlService shortUrlService, UpdateShortUrlRequest request, [FromRoute] long id, 
        ClaimsPrincipal principal, CancellationToken cancellationToken)
    {
        var userId = principal.GetUserId();
        var result = await shortUrlService.UpdateShortUrlAsync(id, request, userId, cancellationToken);
        return result.IsSuccessful
            ? TypedResults.Ok(result.Value!.ToResponse())
            : TypedResults.Problem(result.Error.ToProblemDetails());
    }

    public static async Task<Results<NoContent, ProblemHttpResult>> DeleteShortUrlAsync(
        IShortUrlService shortUrlService, [FromRoute] long id, ClaimsPrincipal principal, CancellationToken cancellationToken)
    {
        var userId = principal.GetUserId();
        var result = await shortUrlService.DeleteShortUrlAsync(id, userId, cancellationToken);
        return result.IsSuccessful 
            ? TypedResults.NoContent() 
            : TypedResults.Problem(result.Error.ToProblemDetails());
    }
}
