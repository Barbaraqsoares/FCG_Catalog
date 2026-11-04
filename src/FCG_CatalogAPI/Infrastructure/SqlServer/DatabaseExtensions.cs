using Microsoft.EntityFrameworkCore;

namespace FCG_CatalogAPI.Infrastructure.SqlServer;

public static class DatabaseExtensions
{
    public static async Task MigrateAndSeedAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DatabaseExtensions");

        var context = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();

        logger.LogInformation("Aplicando migrations pendentes...");

        await context.Database.MigrateAsync();

        await DbInitializer.InitialyzeCatalogAsync(context);

        logger.LogInformation("Banco de dados pronto.");
    }
}