using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using UrlShortener.Api;

var builder = WebApplication.CreateBuilder(args);
// database
builder.Services.AddDbContext<AppDbContext>(opts =>
{
    opts.UseNpgsql(builder.Configuration.GetConnectionString("Default"))
        .UseSnakeCaseNamingConvention();
});
// swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApiDocument(config =>
{
    config.DocumentName = "UrlShortener.API";
    config.Title = "UrlShortener.API v1";
    config.Version = "v1";
});
// CORS
builder.Services.AddCors(options =>
{
    var corsOptions = builder.Configuration.GetSection("Cors").Get<CorsOptions>() 
        ?? throw new InvalidOperationException("Failed to load CORS configuration.");
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(corsOptions.AllowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});
// Id Generator 
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddOptions<IdOptions>()
    .BindConfiguration("IdOptions")
    .ValidateOnStart();
builder.Services.AddSingleton<IValidateOptions<IdOptions>, IdOptionsValidator>();
builder.Services.AddSingleton<IdGenerator<ShortUrl>>();

// Short URL related services
builder.Services.Configure<ApiSettings>(builder.Configuration.GetSection("ApiSettings"));
builder.Services.AddScoped<IValidator<CreateShortUrlRequest>, CreateShortUrlRequestValidator>();
builder.Services.AddScoped<ShortUrlService>();
builder.Services.AddSingleton<ShortUrlClickEventQueue>();
builder.Services.AddHostedService<ShortUrlClickBackgroundService>();

var app = builder.Build();
// CORS
app.UseCors();
// swagger
if (app.Environment.IsDevelopment())
{
    app.UseOpenApi();
    app.UseSwaggerUi(config =>
    {
        config.DocumentTitle = "UrlShortener.API Swagger UI";
        config.DocExpansion = "list";
    });
}

app.MapShortUrlEndpoints();

app.Run();
