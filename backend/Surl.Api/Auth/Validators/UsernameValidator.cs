using FluentValidation;
using Microsoft.Extensions.Localization;
using Surl.Api.Auth.Resources;

namespace Surl.Api.Auth;

public class UsernameValidator : AbstractValidator<string>
{
    public UsernameValidator()
    {
        RuleFor(username => username)
            .NotEmpty()
            .MinimumLength(6)
            .MaximumLength(50)
            .Matches("^[a-zA-Z0-9._-]+$").WithMessage("Username can only contain letters, numbers, underscores, dots and hyphens");
    }
}
