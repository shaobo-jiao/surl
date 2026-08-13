using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Mvc.Testing.Handlers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Time.Testing;
using Npgsql;
using Respawn;
using Surl.Api.Common;
using Surl.Api.Data;
using Surl.Api.ShortUrls;
using Testcontainers.PostgreSql;

namespace Surl.Api.IntegrationTests.Helpers;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder("postgres:18-bookworm")
        .WithDatabase("surl")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    private Respawner _respawner = default!;

    public FakeTimeProvider TimeProvider { get; private set; } = new FakeTimeProvider(TestConstants.TestStartTime);

    public async ValueTask InitializeAsync()
    {
        // start containers
        await _dbContainer.StartAsync();

        // apply migration
        await using var scope = Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await dbContext.Database.MigrateAsync();

        // init respawner 
        await InitializeRespawnerAsync();
    }

    public override async ValueTask DisposeAsync()
    {
        await _dbContainer.DisposeAsync();
        await base.DisposeAsync();
        GC.SuppressFinalize(this);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration(configBuilder =>
        {
            var entries = new Dictionary<string, string?>()
            {
                // db connection string
                ["ConnectionStrings:Default"] = _dbContainer.GetConnectionString(),

                // disallowed host for short url creation
                ["ApiSettings:DisallowedHosts:0"] = "localhost:5001",
                ["ApiSettings:DisallowedHosts:1"] = "localhost:5002",

                // jwt secret
                ["JwtOptions:SecretKey"] = "surl-api-integration-test-jwt-secret-key",
            };
            configBuilder.AddInMemoryCollection(entries);
        });

        builder.ConfigureServices(services =>
        {
            // configure fake time provider
            services.RemoveAll<TimeProvider>();
            services.AddSingleton<TimeProvider>(TimeProvider);

            // configure a custom jwt lifetime validator that is based on TimeProvider 
            services.ConfigureOptions<CustomJwtBearerOptionsLifeTimeValidator>();
        });
    }

    /// <summary>
    /// Initialize the Respawner field that is used to reset database data after every test 
    /// </summary>
    /// <returns></returns>
    public async Task InitializeRespawnerAsync()
    {
        using var dbConnection = new NpgsqlConnection(_dbContainer.GetConnectionString());
        await dbConnection.OpenAsync();
        _respawner = await Respawner.CreateAsync(dbConnection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
        });
    }

    /// <summary>
    /// Resets the database. Called after every test to give a clean context for next test.
    /// </summary>
    /// <returns></returns>
    public async Task ResetStateAsync()
    {
        // reset test start time 
        TimeProvider.AdjustTime(TestConstants.TestStartTime);

        // reset db data
        using var dbConnection = new NpgsqlConnection(_dbContainer.GetConnectionString());
        await dbConnection.OpenAsync();
        await _respawner.ResetAsync(dbConnection);

        // reset Id Generator sequence
        Services.GetRequiredService<IdGenerator<ShortUrl>>()
            .Reset();
    }

    /// <summary>
    /// Creates a default HttpClient instance together with a CookieContainer instance to access cookies.
    /// </summary>
    /// <returns></returns>
    public (HttpClient, CookieContainer) CreateClientWithCookieContainer()
    {
        var cookieContainer = new CookieContainer();
        var cookieContainerHandler = new CookieContainerHandler(cookieContainer);
        
        var httpClient = CreateDefaultClient(new Uri(TestConstants.TestBaseAddress), cookieContainerHandler);
        return (httpClient, cookieContainer);
    }

    /// <summary>
    /// Creates a HttpClient instance that handles cookie and uses https base address "https://localhost".
    /// </summary>
    /// <returns></returns>
    public new HttpClient CreateClient()
    {
        return CreateClient(new WebApplicationFactoryClientOptions()
        {
            // use https so that refresh token cookie (secure = true) can be sent.
            BaseAddress = new Uri(TestConstants.TestBaseAddress),
            HandleCookies = true, 
        });
    }
}
