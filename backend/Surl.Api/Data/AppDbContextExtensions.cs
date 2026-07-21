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
            var passwordFile = configuration.GetValue<string>("Secrets:DB_Default_Password_File");
            if (File.Exists(passwordFile))
            {
                connectionStringBuilder.Password = File.ReadAllText(passwordFile).TrimEnd();
            }
            
            // postgres database
            opts.UseNpgsql(connectionStringBuilder.ConnectionString)
                .UseSnakeCaseNamingConvention();

            // seed a default Admin;
            var defaultAdminUsername = configuration["DefaultAdmin:Username"] ?? throw new InvalidOperationException("Default admin username not configured");
            var defaultAdminPassword = configuration["DefaultAdmin:Password"] ?? throw new InvalidOperationException("Default admin password not configured");

            opts.UseSeeding((context, _) =>
            {
                if (!context.Set<User>().Any(u => u.Username == defaultAdminUsername))
                {
                    var user = new User(defaultAdminUsername);
                    user.PasswordHash = new PasswordHasher<User>().HashPassword(user, defaultAdminPassword);
                    user.Role = AppRoles.Admin;
                    context.Set<User>().Add(user);
                    context.SaveChanges();
                }
            })
            .UseAsyncSeeding(async (context, _, cancellationToken) =>
            {
                if (!await context.Set<User>().AnyAsync(u => u.Username == defaultAdminUsername, cancellationToken))
                {
                    var user = new User(defaultAdminUsername);
                    user.PasswordHash = new PasswordHasher<User>().HashPassword(user, defaultAdminPassword);
                    user.Role = AppRoles.Admin;
                    await context.Set<User>().AddAsync(user);
                    await context.SaveChangesAsync();
                }
            });
        });
    }
}
