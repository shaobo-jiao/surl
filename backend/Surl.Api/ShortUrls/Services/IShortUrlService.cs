using Surl.Api.Common;

namespace Surl.Api.ShortUrls;

public interface IShortUrlService
{
    /// <summary>
    /// Creates a new short url. The created URL may be linked to the created user if authenticated, 
    /// or standalone if created from "Try It Out". 
    /// </summary>
    /// <param name="request"></param>
    /// <param name="userId">optional. may be null if from Try It Out</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ShortUrl> CreateShortUrlAsync(CreateShortUrlRequest request, int? userId, CancellationToken cancellationToken);

    /// <summary>
    /// Gets a short URL by its ID.
    /// </summary>
    /// <param name="shortUrlId"></param>
    /// <param name="userId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ShortUrl?> GetShortUrlByIdAsync(long shortUrlId, int? userId, CancellationToken cancellationToken);

    /// <summary>
    /// Gets list of short URLs using given query parameters. 
    /// </summary>
    /// <param name="queryParams"></param>
    /// <param name="userId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<PagedResult<ShortUrl>> GetShortUrlsAsync(QueryShortUrlParams queryParams, int? userId, CancellationToken cancellationToken);

    /// <summary>
    /// Gets original URL for redirection from given short url code. 
    /// Returns null if the URL is not found. 
    /// Throws errors if the original URL is invalid. 
    /// </summary>
    /// <param name="shortUrlCode"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<Result<ShortUrl, ShortUrlError>> GetRedirectionUrlAsync(string shortUrlCode, CancellationToken cancellationToken);

    /// <summary>
    /// Updates a short URL.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="userId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<Result<ShortUrl, ShortUrlError>> UpdateShortUrlAsync(long shortUrlId, UpdateShortUrlRequest request, int? userId, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes a short URL by its ID. 
    /// </summary>
    /// <param name="shortUrlId"></param>
    /// <param name="userId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<Result<ShortUrl, ShortUrlError>> DeleteShortUrlAsync(long shortUrlId, int? userId, CancellationToken cancellationToken);

    /// <summary>
    /// Saves a click event. To be called by the click processing background service
    /// </summary>
    /// <param name="clickEvent"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task SaveClickEventAsync(ShortUrlClickEvent clickEvent, CancellationToken cancellationToken);
}
