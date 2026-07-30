using FluentValidation;
using Surl.Api.Auth;

namespace Surl.Api.Users;

public class UpdateUserRequestValidator: AbstractValidator<UpdateUserRequest>
{
    public UpdateUserRequestValidator(UsernameValidator usernameValidator, PasswordValidator passwordValidator)
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
