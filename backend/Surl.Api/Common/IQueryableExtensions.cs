using System.Linq.Expressions;

namespace Surl.Api.Common;

public static class IQueryableExtensions
{
    public static IOrderedQueryable<T> OrderByDirection<T, TKey>(this IQueryable<T> query, 
        Expression<Func<T, TKey>> keySelector, bool isDescending)
    {
        return isDescending 
            ? query.OrderByDescending(keySelector)
            : query.OrderBy(keySelector);
    }

    public static IOrderedQueryable<T> ThenByDirection<T, TKey>(this IOrderedQueryable<T> query,
        Expression<Func<T, TKey>> keySelector, bool isDescending)
    {
        return isDescending
            ? query.ThenByDescending(keySelector)
            : query.ThenBy(keySelector);
    }
}
