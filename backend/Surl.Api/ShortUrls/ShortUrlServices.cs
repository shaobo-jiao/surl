using FluentValidation;
using Surl.Api.Common;

namespace Surl.Api.ShortUrls;

public static class ShortUrlServices
{
    /// <summary>
    /// Registers all services required by the ShortUrls module
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    public static void AddShortUrlServices(this IServiceCollection services)
    {   
        services.AddSingleton<IdGenerator<ShortUrl>>();
        services.AddOptions<ApiSettings>().BindConfiguration("ApiSettings");
        
        services.AddScoped<ShortUrlService>();
        services.AddScoped<IValidator<CreateShortUrlRequest>, CreateShortUrlRequestValidator>();
        
        services.AddSingleton<ShortUrlClickEventQueue>();
        services.AddHostedService<ShortUrlClickBackgroundService>();
    }
}
