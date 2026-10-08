using FluentValidation;

namespace Surl.Api.ShortUrls;

public class UpdateShortUrlRequestValidator : AbstractValidator<UpdateShortUrlRequest>
{
    public UpdateShortUrlRequestValidator(TimeProvider timeProvider)
    {
        RuleFor(x => x.ExpiresAt)
            .Must(expiresAt => expiresAt is null || expiresAt > timeProvider.GetUtcNow())
            .WithMessage("The expiry date must be in the future");
    }
}
