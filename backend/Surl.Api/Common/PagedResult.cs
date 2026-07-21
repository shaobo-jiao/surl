using Microsoft.EntityFrameworkCore;

namespace Surl.Api.Common;

/// <summary>
/// Represents a paginated result set when returning a data list. 
/// </summary>
public record PagedResult<T>(IEnumerable<T> Items, int Page, int PageSize,int TotalCount)
{
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}

public static class PagedResultExtensions
{
    /// <summary>
    /// Queries the data and wraps the result in a paginated result set.
    /// </summary>
    public static async Task<PagedResult<T>> ToPagedResultAsync<T>(this IQueryable<T> query, int? page, int? pageSize)
    {
        page ??= PagedResultConstants.DefaultPage;
        pageSize ??= PagedResultConstants.DefaultPageSize;

        var totalCount = await query.CountAsync();
        var items = await query.Skip((page.Value - 1) * pageSize.Value)
            .Take(pageSize.Value)
            .ToListAsync();

        return new PagedResult<T>(items, page.Value, pageSize.Value, totalCount);
    }

    /// <summary>
    /// Maps items in a paginated result set to a different type using the provided selector. 
    /// A typical use case is to map from an entity type to a response DTO type.
    /// </summary>
    public static PagedResult<TResult> Map<TSource, TResult>(this PagedResult<TSource> pagedResult, Func<TSource, TResult> selector)
    {
        var mappedItems = pagedResult.Items.Select(selector);
        return new PagedResult<TResult>(mappedItems, pagedResult.Page, pagedResult.PageSize, pagedResult.TotalCount);
    }
}
