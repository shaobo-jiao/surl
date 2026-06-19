using FluentValidation;
using Microsoft.Extensions.Options;

namespace UrlShortener.Api;

public record CreateShortUrlRequest(string OriginalUrl);

public class CreateShortUrlRequestValidator: AbstractValidator<CreateShortUrlRequest>
{
    public CreateShortUrlRequestValidator(IOptions<ApiSettings> apiSettingsOption)
    {
        var apiSettings = apiSettingsOption.Value;

        // validate the original url is valid, and has allowed hosts
        RuleFor(x => x.OriginalUrl)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .Must(s => Uri.TryCreate(s, UriKind.Absolute, out var url) && 
                (url.Scheme == Uri.UriSchemeHttp || url.Scheme == Uri.UriSchemeHttps))
            .WithMessage("The URL is invalid")
            .Must(s => !apiSettings.DisallowedHosts.Contains(new Uri(s).Authority))
            .WithMessage("The URL host is not allowed");
    }
}