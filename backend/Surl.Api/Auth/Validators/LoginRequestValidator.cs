using FluentValidation;
using Microsoft.Extensions.Localization;
using Surl.Api.Auth.Resources;

namespace Surl.Api.Auth;

public class LoginRequestValidator: AbstractValidator<LoginRequest>
{
    public LoginRequestValidator(IStringLocalizer<AuthResources> localizer)
    {
        RuleFor(x => x.Username)
            .NotEmpty()
            .MinimumLength(3)
            .MaximumLength(20)
            .Matches("^[a-zA-Z0-9._-]+$").WithMessage(localizer["Auth_Username_InvalidFormat"]);

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(6)
            .MaximumLength(100);
    }
}
