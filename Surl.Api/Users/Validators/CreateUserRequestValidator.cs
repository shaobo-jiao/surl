using FluentValidation;
using Microsoft.Extensions.Localization;
using Surl.Api.Users.Resources;

namespace Surl.Api.Users;

public class CreateUserRequestValidator: AbstractValidator<CreateUserRequest>
{
    public CreateUserRequestValidator(IStringLocalizer<UserResources> localizer)
    {
        RuleFor(x => x.Username)
            .NotEmpty()
            .MinimumLength(3)
            .MaximumLength(20)
            .Matches("^[a-zA-Z0-9._-]+$").WithMessage(localizer["User_Username_InvalidFormat"]);

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(6)
            .MaximumLength(100);

        RuleFor(x => x.Email)
            .EmailAddress()
            .When(x => !string.IsNullOrEmpty(x.Email));

        RuleFor(x => x.Role)
            .Must(x => Roles.All().Contains(x))
            .WithMessage(string.Format(localizer["User_Role_Invalid"], string.Join(", ", Roles.All())));
    }
}
