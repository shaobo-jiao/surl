using Microsoft.EntityFrameworkCore;

namespace Surl.Api.ShortUrls;

public static class ModelConfiguration
{
    /// <summary>
    /// Configures entities in the ShortUrls module.
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static ModelBuilder ConfigureShortUrlEntities(this ModelBuilder builder)
    {
        builder.Entity<ShortUrl>()
            .Property(e => e.Id)
            .ValueGeneratedNever();

        builder.Entity<ShortUrl>()
            .HasMany(e => e.Clicks)
            .WithOne(e => e.ShortUrl)
            .HasForeignKey(e => e.ShortUrlId)
            .IsRequired();

        builder.Entity<ShortUrl>()
            .HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        return builder;
    }
}
