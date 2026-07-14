using FluentValidation;

namespace Surl.Api.Auth;

public class RefreshRequestValidator: AbstractValidator<RefreshRequest>
{
    public RefreshRequestValidator()
    {
        RuleFor(r => r.RefreshToken).NotEmpty();
    }
}
