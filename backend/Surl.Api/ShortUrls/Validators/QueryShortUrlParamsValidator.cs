using FluentValidation;

namespace Surl.Api.ShortUrls;

public class QueryShortUrlParamsValidator : AbstractValidator<QueryShortUrlParams>
{
    public QueryShortUrlParamsValidator()
    {
        var allowOrderByProperties = QueryShortUrlAllowedOrderByProperties.All();
        RuleFor(x => x.OrderBy)
            .Must(x => x == null || allowOrderByProperties.Contains(x, StringComparer.InvariantCultureIgnoreCase))
            .WithMessage(string.Format("OrderBy can only be one of: {0}", string.Join(", ", allowOrderByProperties)));
        
        RuleFor(x => x.Page).GreaterThan(0);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}
