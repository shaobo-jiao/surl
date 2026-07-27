using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Time.Testing;
using Npgsql;
using Respawn;
using Surl.Api.Data;
using Testcontainers.PostgreSql;

namespace Surl.Api.IntegrationTests.Common;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder("postgres:18-bookworm")
        .WithDatabase("surl")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    private Respawner _respawner = default!;

    public HttpClient HttpClient { get; private set; } = default!;

    public FakeTimeProvider TimeProvider { get; private set; } = new FakeTimeProvider(TestConstants.TestStartTime);

    public async ValueTask InitializeAsync()
    {
        // start containers
        await _dbContainer.StartAsync();

        // start application
        HttpClient = CreateClient();

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
            };
            configBuilder.AddInMemoryCollection(entries);
        });

        builder.ConfigureServices(services =>
        {
            // configure fake time provider
            services.RemoveAll<TimeProvider>();
            services.AddSingleton<TimeProvider>(TimeProvider);
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
        // reset time 
        TimeProvider.SetUtcNow(TestConstants.TestStartTime);

        // reset db data
        using var dbConnection = new NpgsqlConnection(_dbContainer.GetConnectionString());
        await dbConnection.OpenAsync();
        await _respawner.ResetAsync(dbConnection);
    }
}
