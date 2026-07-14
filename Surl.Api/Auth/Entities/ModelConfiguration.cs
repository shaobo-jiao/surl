using Microsoft.EntityFrameworkCore;

namespace Surl.Api.Auth;

public static class ModelConfiguration
{
    /// <summary>
    /// Configures entities in the Auth module.
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static ModelBuilder ConfigureAuthEntities(this ModelBuilder builder)
    {
        builder.Entity<RefreshToken>(refreshToken =>
        {
            refreshToken.HasOne(t => t.User)
                .WithMany()
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        return builder;
    }
}