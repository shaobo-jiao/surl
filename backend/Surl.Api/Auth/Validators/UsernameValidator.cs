using FluentValidation;
using Microsoft.Extensions.Localization;
using Surl.Api.Auth.Resources;

namespace Surl.Api.Auth;

public class UsernameValidator : AbstractValidator<string>
{
    public UsernameValidator(IStringLocalizer<AuthResources> localizer)
    {
        RuleFor(username => username)
            .NotEmpty()
            .MinimumLength(3)
            .MaximumLength(20)
            .Matches("^[a-zA-Z0-9._-]+$").WithMessage(localizer["Auth_Username_InvalidFormat"]);
    }
}
