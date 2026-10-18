using Microsoft.EntityFrameworkCore;

namespace FCG_CatalogAPI.Infrastructure.SqlServer;

public class CatalogDbContext(DbContextOptions<CatalogDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}