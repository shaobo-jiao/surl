using FluentValidation;
using Microsoft.Extensions.Localization;
using Surl.Api.Auth.Resources;

namespace Surl.Api.Auth;

public class LoginRequestValidator: AbstractValidator<LoginRequest>
{
    public LoginRequestValidator(IStringLocalizer<AuthResources> localizer)
    {
        RuleFor(x => x.Username).NotEmpty();
        RuleFor(x => x.Password).NotEmpty();
    }
}
