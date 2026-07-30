using FluentValidation;

namespace Surl.Api.Auth;

public class PasswordValidator : AbstractValidator<string>
{
    public PasswordValidator()
    {
        RuleFor(password => password)
            .NotEmpty()
            .MinimumLength(6)
            .MaximumLength(100);
    }
}
