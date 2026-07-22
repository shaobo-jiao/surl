using System.Linq.Expressions;

namespace Surl.Api.Common;

public static class IQueryableExtensions
{
    public static IOrderedQueryable<T> OrderByDirection<T, TKey>(this IQueryable<T> query, 
        Expression<Func<T, TKey>> keySelector, bool IsDescending)
    {
        return IsDescending 
            ? query.OrderByDescending(keySelector)
            : query.OrderBy(keySelector);
    }
}
