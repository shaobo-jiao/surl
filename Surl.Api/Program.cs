using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Surl.Api.Auth;
using Surl.Api.Common;
using Surl.Api.Data;
using Surl.Api.ShortUrls;
using Surl.Api.Users;

var builder = WebApplication.CreateBuilder(args);
// database
builder.Services.AddAppDbContext(builder.Configuration);

// swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApiDocument(config =>
{
    config.DocumentName = "Surl.API";
    config.Title = "Surl.API v1";
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

// localization
builder.Services.AddLocalization();
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[] { "en" };
    options.SetDefaultCulture(supportedCultures[0])
        .AddSupportedCultures(supportedCultures)
        .AddSupportedUICultures(supportedCultures);
});

// Id Generator Base
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddOptions<IdOptions>()
    .BindConfiguration("IdOptions")
    .ValidateOnStart();
builder.Services.AddSingleton<IValidateOptions<IdOptions>, IdOptionsValidator>();

// Module Services
builder.Services.AddShortUrlServices(builder.Configuration);
builder.Services.AddUserServices();
builder.Services.AddAuthServices(builder.Configuration);

var app = builder.Build();
// CORS
app.UseCors();
// swagger
if (app.Environment.IsDevelopment())
{
    app.UseOpenApi();
    app.UseSwaggerUi(config =>
    {
        config.DocumentTitle = "Surl.API Swagger UI";
        config.DocExpansion = "list";
    });
}
app.UseRequestLocalization();

app.UseAuthentication();
app.UseAuthorization();

// endpoints
app.MapShortUrlEndpoints();
app.MapUserEndpoints();
app.MapAuthEndpoints();

app.Run();
