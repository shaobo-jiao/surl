using FluentValidation;
using Microsoft.Extensions.Localization;
using Surl.Api.Users.Resources;

namespace Surl.Api.Users;

public class QueryUserParamsValidator : AbstractValidator<QueryUserParams>
{   
    public QueryUserParamsValidator(IStringLocalizer<UserResources> localizer)
    {
        var allowedProperties = QueryUserParamsAllowedOrderByProperties.All();
        
        RuleFor(x => x.OrderBy)
            .Must(x => x == null || allowedProperties.Contains(x))
            .WithMessage(string.Format(localizer["User_OrderBy_Invalid"], string.Join(", ", allowedProperties)));

        RuleFor(x => x.Page).GreaterThan(0);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}
