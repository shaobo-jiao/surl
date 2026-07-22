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
            .WithValidationFilter<QueryShortUrlParams>() //TODO: add validator
            .RequireAuthorization();

        shortUrlApi.MapGet("/{id:long}", GetShortUrlByIdAsync)
            .RequireAuthorization();
        
        shortUrlApi.MapPatch("/{id:long}", UpdateShortUrlAsync)
            .WithValidationFilter<UpdateShortUrlRequest>() //TODO: add validator
            .RequireAuthorization();
        
        shortUrlApi.MapDelete("/{id:long}", DeleteShortUrlAsync)
            .RequireAuthorization();

        app.MapGet("/redirection/{code}", GetRedirectionUrlAsync)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status410Gone)
            .ProducesProblem(StatusCodes.Status400BadRequest);
    }

    public static async Task<Results<CreatedAtRoute<ShortUrlResponse>, BadRequest>> CreateShortUrlAsync(
        IShortUrlService shortUrlSvc, CreateShortUrlRequest request, ClaimsPrincipal principal, CancellationToken cancellationToken)
    {
        var userId = principal.GetUserId();
        var shortUrl = await shortUrlSvc.CreateShortUrlAsync(request, userId, cancellationToken);
        var response = shortUrl.ToResponse();
        return TypedResults.CreatedAtRoute(response, "GetOriginalUrl", new { code = shortUrl.Code });
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
    public static async Task<Results<Ok<string>, NotFound, BadRequest<string>>> GetRedirectionUrlAsync(
        IShortUrlService shortUrlService, [FromRoute] string code, ShortUrlClickEventQueue clickEventQueue, CancellationToken cancellationToken)
    {
        try
        {
            var shortUrl = await shortUrlService.GetRedirectionUrlAsync(code, cancellationToken);
            if (shortUrl is null) return TypedResults.NotFound();

            var clickEvent = new ShortUrlClickEvent(shortUrl.Id, shortUrl.Code, DateTimeOffset.UtcNow);
            await clickEventQueue.EnqueueAsync(clickEvent, cancellationToken);

            return TypedResults.Ok(shortUrl.OriginalUrl);
        }
        catch (ShortUrlBaseException ex)
        {
            return TypedResults.BadRequest(ex.Message);
        }
    }

    public static async Task<Results<Ok<ShortUrlResponse>, NotFound>> UpdateShortUrlAsync(
        IShortUrlService shortUrlService, UpdateShortUrlRequest request, [FromRoute] long id, 
        ClaimsPrincipal principal, CancellationToken cancellationToken)
    {
        var userId = principal.GetUserId();
        var shorturl = await shortUrlService.UpdateShortUrlAsync(id, request, userId, cancellationToken);
        return TypedResults.Ok(shorturl.ToResponse());
    }

    public static async Task<Results<NoContent, BadRequest<string>>> DeleteShortUrlAsync(
        IShortUrlService shortUrlService, [FromRoute] long id, ClaimsPrincipal principal, CancellationToken cancellationToken)
    {
        try
        {
            var userId = principal.GetUserId();
            await shortUrlService.DeleteShortUrlAsync(id, userId, cancellationToken);
            return TypedResults.NoContent();
        }
        catch (ShortUrlBaseException ex)
        {
            return TypedResults.BadRequest(ex.Message);
        }
    }
}
