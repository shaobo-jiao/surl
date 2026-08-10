using FluentValidation;
using Surl.Api.Auth;

namespace Surl.Api.Users;

public class UpdateUserRequestValidator: AbstractValidator<UpdateUserRequest>
{
    public UpdateUserRequestValidator()
    {
        RuleFor(x => x.Username).Username();

        RuleFor(x => x.Password)
            .Cascade(CascadeMode.Stop)
            .StrongPassword();

        RuleFor(x => x.Email)
            .EmailAddress()
            .When(x => !string.IsNullOrEmpty(x.Email));
    }
}
