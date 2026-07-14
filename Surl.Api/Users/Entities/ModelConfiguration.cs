using Microsoft.EntityFrameworkCore;

namespace Surl.Api.Users;

public static class ModelConfiguration
{
    /// <summary>
    /// Configures entities in the User module.
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static ModelBuilder ConfigureUserEntities(this ModelBuilder builder)
    {
        builder.Entity<User>(user =>
        {
            user.HasIndex(u => u.Username).IsUnique();
        });

        return builder;
    }
}