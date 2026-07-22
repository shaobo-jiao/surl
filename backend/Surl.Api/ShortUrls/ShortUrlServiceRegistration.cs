using FluentValidation;
using Surl.Api.Common;

namespace Surl.Api.ShortUrls;

public static class ShortUrlServiceRegistration
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
        
        services.AddScoped<IShortUrlService, ShortUrlService>();
        services.AddScoped<IValidator<CreateShortUrlRequest>, CreateShortUrlRequestValidator>();
        services.AddScoped<IValidator<UpdateShortUrlRequest>, UpdateShortUrlRequestValidator>();
        services.AddScoped<IValidator<QueryShortUrlParams>, QueryShortUrlParamsValidator>();
        
        services.AddSingleton<ShortUrlClickEventQueue>();
        services.AddHostedService<ShortUrlClickBackgroundService>();
    }
}
