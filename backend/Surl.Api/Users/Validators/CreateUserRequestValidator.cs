using FluentValidation;
using Microsoft.Extensions.Localization;
using Surl.Api.Auth;
using Surl.Api.Users.Resources;

namespace Surl.Api.Users;

public class CreateUserRequestValidator: AbstractValidator<CreateUserRequest>
{
    public CreateUserRequestValidator(IStringLocalizer<UserResources> localizer,
        UsernameValidator usernameValidator, PasswordValidator passwordValidator)
    {
        RuleFor(x => x.Username)
            .SetValidator(usernameValidator);

        RuleFor(x => x.Password)
            .SetValidator(passwordValidator);

        RuleFor(x => x.Email)
            .EmailAddress()
            .When(x => !string.IsNullOrEmpty(x.Email));

        RuleFor(x => x.Role)
            .Must(x => AppRoles.All().Contains(x))
            .WithMessage(string.Format(localizer["User_Role_Invalid"], string.Join(", ", AppRoles.All())));
    }
}
