using Microsoft.EntityFrameworkCore;
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
// global settings
builder.Services.AddOptions<ApiSettings>()
    .BindConfiguration("ApiSettings")
    .ValidateDataAnnotations()
    .ValidateOnStart();
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
// Short URL related services
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
