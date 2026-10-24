using FCG_CatalogAPI.Domain.Entities;
using FCG_CatalogAPI.Infrastructure.SqlServer;
using Microsoft.EntityFrameworkCore;

namespace FCG_CatalogAPI.Tests;

internal static class TestData
{
    public static Game Game(int id = 1, string name = "Game", decimal price = 25m) => new()
    {
        Id = id,
        Name = name,
        Description = "Description",
        Price = price,
        IsActive = true,
        IsMultiplayer = false,
        CreatedAt = new DateTime(2025, 1, 1),
        UpdatedAt = new DateTime(2025, 1, 2)
    };

    public static Promotion Promotion(int id = 1, string name = "Promotion", params Game[] games) => new()
    {
        Id = id,
        Name = name,
        Discount = 10,
        StartDate = new DateTime(2025, 2, 1),
        EndDate = new DateTime(2025, 2, 28),
        IsActive = true,
        CreatedByUserId = 7,
        CreatedAt = new DateTime(2025, 1, 1),
        Games = games.ToList()
    };

    public static CatalogDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<CatalogDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new CatalogDbContext(options);
    }
}
