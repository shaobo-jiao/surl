using FluentValidation;

namespace Surl.Api.Auth;

public class ChangePasswordRequestValidator: AbstractValidator<ChangePasswordRequest>
{
    public ChangePasswordRequestValidator(PasswordValidator passwordValidator, StrongPasswordValidator strongPasswordValidator)
    {
        RuleFor(r => r.OldPassword)
            .SetValidator(passwordValidator);

        RuleFor(r => r.NewPassword)
            .SetValidator(strongPasswordValidator);
    }
}
