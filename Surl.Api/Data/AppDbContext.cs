using Microsoft.EntityFrameworkCore;
using Surl.Api.Auth;
using Surl.Api.ShortUrls;
using Surl.Api.Users;

namespace Surl.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<ShortUrl> ShortUrls { get; set; }
    public DbSet<ShortUrlClick> ShortUrlClicks { get; set; }

    public DbSet<User> Users { get; set; }
    
    public DbSet<RefreshToken> RefreshTokens { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ConfigureShortUrlEntities();
        modelBuilder.ConfigureUserEntities();
        modelBuilder.ConfigureAuthEntities();
    }
}
