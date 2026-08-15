using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Surl.Api.Users;

namespace Surl.Api.Data;

public static class AppDbContextExtensions
{
    public static void AddAppDbContext(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(opts =>
        {
            // build connection string
            var connectionStringBuilder = new NpgsqlConnectionStringBuilder(configuration.GetConnectionString("Default"));
            // get password from secret file if exists
            var passwordFile = configuration.GetValue<string>("Secrets:DbDefaultPasswordFile");
            if (File.Exists(passwordFile))
            {
                connectionStringBuilder.Password = File.ReadAllText(passwordFile).TrimEnd();
            }
            
            // postgres database
            opts.UseNpgsql(connectionStringBuilder.ConnectionString)
                .UseSnakeCaseNamingConvention();

            // seed a default Admin;
            // NOTE: this is applied when applying migration with `dotnet ef database update`, but not with migration scripts.
            var defaultAdminUsername = configuration["DefaultAdmin:Username"] ?? throw new InvalidOperationException("Default admin username not configured");
            var defaultAdminPassword = configuration["DefaultAdmin:Password"] ?? throw new InvalidOperationException("Default admin password not configured");

            opts.UseSeeding((context, _) =>
            {
                if (!context.Set<User>().Any(u => u.Username == defaultAdminUsername))
                {
                    var user = new User(defaultAdminUsername)
                    {
                        CreatedAt = new DateTimeOffset(2001, 1, 1, 0, 0, 0, TimeSpan.Zero), // default CreatedAt
                        Role = AppRoles.Admin,
                    };
                    user.PasswordHash = new PasswordHasher<User>().HashPassword(user, defaultAdminPassword);
                    context.Set<User>().Add(user);
                    context.SaveChanges();
                }
            })
            .UseAsyncSeeding(async (context, _, cancellationToken) =>
            {
                if (!await context.Set<User>().AnyAsync(u => u.Username == defaultAdminUsername, cancellationToken))
                {
                    var user = new User(defaultAdminUsername)
                    {
                        CreatedAt = new DateTimeOffset(2001, 1, 1, 0, 0, 0, TimeSpan.Zero), // default CreatedAt
                        Role = AppRoles.Admin,
                    };
                    user.PasswordHash = new PasswordHasher<User>().HashPassword(user, defaultAdminPassword);
                    await context.Set<User>().AddAsync(user, cancellationToken);
                    await context.SaveChangesAsync(cancellationToken);
                }
            });
        });
    }
}
