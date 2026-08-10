using FluentValidation;
using Surl.Api.Auth;

namespace Surl.Api.Users;

public class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserRequestValidator()
    {
        RuleFor(x => x.Username).Username();

        RuleFor(x => x.Password)
            .Cascade(CascadeMode.Stop)
            .StrongPassword();

        RuleFor(x => x.Email)
            .EmailAddress()
            .When(x => !string.IsNullOrEmpty(x.Email));

        RuleFor(x => x.Role)
            .Must(x => AppRoles.All().Contains(x))
            .WithMessage(string.Format("Role must be one of: {0}", string.Join(", ", AppRoles.All())));
    }
}
