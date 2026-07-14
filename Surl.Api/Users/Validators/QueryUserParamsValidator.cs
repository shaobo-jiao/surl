using FluentValidation;
using Microsoft.Extensions.Localization;
using Surl.Api.Users.Resources;

namespace Surl.Api.Users;

public class QueryUserParamsValidator : AbstractValidator<QueryUserParams>
{
    private HashSet<string> _allowedOrderByFields = new()
    {
        nameof(User.Id),
        nameof(User.Username),
        nameof(User.Email),
        nameof(User.CreatedAt)
    };
    
    public QueryUserParamsValidator(IStringLocalizer<UserResources> localizer)
    {
        RuleFor(x => x.OrderBy)
            .Must(x => x == null || _allowedOrderByFields.Contains(x))
            .WithMessage(string.Format(localizer["User_OrderBy_Invalid"], string.Join(", ", _allowedOrderByFields)));
        RuleFor(x => x.Page).GreaterThan(0);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}
