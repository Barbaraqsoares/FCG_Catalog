using FCG_CatalogAPI.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FCG_CatalogAPI.Infrastructure.SqlServer;

public class CatalogDbContext(DbContextOptions<CatalogDbContext> options) : DbContext(options)
{
    public DbSet<Game> Games { get; set; }
    public DbSet<Promotion> Promotions { get; set; }
    public DbSet<UserGame> UserGames { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}