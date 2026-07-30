using FluentValidation;

namespace Surl.Api.Auth;

public class LoginRequestValidator: AbstractValidator<LoginRequest>
{
    public LoginRequestValidator(UsernameValidator usernameValidator, PasswordValidator passwordValidator)
    {
        RuleFor(x => x.Username)
            .SetValidator(usernameValidator);

        RuleFor(x => x.Password)
            .SetValidator(passwordValidator);
    }
}
