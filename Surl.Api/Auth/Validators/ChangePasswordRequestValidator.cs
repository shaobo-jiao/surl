using FluentValidation;
using Microsoft.Extensions.Localization;
using Surl.Api.Auth.Resources;

namespace Surl.Api.Auth;

public class ChangePasswordRequestValidator: AbstractValidator<ChangePasswordRequest>
{
    public ChangePasswordRequestValidator(IStringLocalizer<AuthResources> localizer)
    {
        RuleFor(r => r.OldPassword)
            .NotEmpty();

        RuleFor(r => r.NewPassword)
            .NotEmpty()
            .MinimumLength(6)
            .MaximumLength(100);
    }
}
