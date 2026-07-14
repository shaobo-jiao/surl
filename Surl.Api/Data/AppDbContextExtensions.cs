using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Surl.Api.Users;

namespace Surl.Api.Data;

public static class AppDbContextExtensions
{
    public static void AddAppDbContext(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(opts =>
        {
            // postgres database
            opts.UseNpgsql(configuration.GetConnectionString("Default"))
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
                    user.Role = Roles.Admin;
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
                    user.Role = Roles.Admin;
                    await context.Set<User>().AddAsync(user);
                    await context.SaveChangesAsync();
                }
            });
        });
    }
}
