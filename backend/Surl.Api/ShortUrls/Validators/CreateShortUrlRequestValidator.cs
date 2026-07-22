using FluentValidation;
using Microsoft.Extensions.Options;
using Surl.Api.Common;

namespace Surl.Api.ShortUrls;

public class CreateShortUrlRequestValidator : AbstractValidator<CreateShortUrlRequest>
{
    public CreateShortUrlRequestValidator(IOptions<ApiSettings> apiSettingsWrapper, TimeProvider timeProvider)
    {
        var apiSettings = apiSettingsWrapper.Value;

        // validate the original url is valid, and has allowed hosts
        RuleFor(x => x.OriginalUrl)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .Must(s => Uri.TryCreate(s, UriKind.Absolute, out var url) &&
                (url.Scheme == Uri.UriSchemeHttp || url.Scheme == Uri.UriSchemeHttps))
            .WithMessage("The URL is invalid")
            .Must(s => !apiSettings.DisallowedHosts.Contains(new Uri(s).Authority))
            .WithMessage("The URL host is not allowed");

        RuleFor(x => x.ExpiresAt)
            .Must(expiresAt => expiresAt is null || expiresAt > timeProvider.GetUtcNow())
            .WithMessage("The expiry date must be in the future");
    }
}
