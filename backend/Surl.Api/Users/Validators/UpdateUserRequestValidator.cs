using FluentValidation;
using Surl.Api.Auth;

namespace Surl.Api.Users;

public class UpdateUserRequestValidator: AbstractValidator<UpdateUserRequest>
{
    public UpdateUserRequestValidator(UsernameValidator usernameValidator, StrongPasswordValidator strongPasswordValidator)
    {
        RuleFor(x => x.Username)
            .SetValidator(usernameValidator);

        RuleFor(x => x.Password)
            .SetValidator(strongPasswordValidator);

        RuleFor(x => x.Email)
            .EmailAddress()
            .When(x => !string.IsNullOrEmpty(x.Email));
    }
}
