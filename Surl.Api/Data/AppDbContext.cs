using Microsoft.EntityFrameworkCore;

namespace Surl.Api;

public class AppDbContext: DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<ShortUrl> ShortUrls { get; set; }
    public DbSet<ShortUrlClick> ShortUrlClicks { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ShortUrl>()
            .Property(e => e.Id)
            .ValueGeneratedNever();
        
        modelBuilder.Entity<ShortUrl>()
            .HasMany(e => e.Clicks)
            .WithOne(e => e.ShortUrl)
            .HasForeignKey(e => e.ShortUrlId)
            .IsRequired();
    }
}
