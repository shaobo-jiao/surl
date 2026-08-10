using FluentValidation;

namespace Surl.Api.Auth;

public class LoginRequestValidator: AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Username)
            .Username();

        RuleFor(x => x.Password)
            .Cascade(CascadeMode.Stop)
            .Password();
    }
}
