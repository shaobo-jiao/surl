using FluentValidation;

namespace Surl.Api.Auth;

public class ChangePasswordRequestValidator: AbstractValidator<ChangePasswordRequest>
{
    public ChangePasswordRequestValidator()
    {
        RuleFor(r => r.OldPassword).Password();

        RuleFor(r => r.NewPassword)
            .Cascade(CascadeMode.Stop)
            .StrongPassword();
    }
}
