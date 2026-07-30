using FluentValidation;

namespace Surl.Api.Auth;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator(UsernameValidator usernameValidator, PasswordValidator passwordValidator)
    {
        RuleFor(x => x.Username)
            .SetValidator(usernameValidator);

        RuleFor(x => x.Password)
            .SetValidator(passwordValidator);

        RuleFor(x => x.Email)
            .EmailAddress()
            .When(x => !string.IsNullOrEmpty(x.Email));
    }
}
