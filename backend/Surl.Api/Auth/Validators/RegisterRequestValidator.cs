using FluentValidation;

namespace Surl.Api.Auth;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Username)
            .Username();

        RuleFor(x => x.Password)
            .Cascade(CascadeMode.Stop)
            .StrongPassword();

        RuleFor(x => x.Email)
            .EmailAddress()
            .When(x => !string.IsNullOrEmpty(x.Email));
    }
}
