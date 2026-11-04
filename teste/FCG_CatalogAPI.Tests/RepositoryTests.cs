using FCG_CatalogAPI.Adapters.Outbound;
using FCG_CatalogAPI.Domain.Entities;
using FCG_CatalogAPI.Infrastructure.SqlServer;
using Xunit;

namespace FCG_CatalogAPI.Tests;

public class RepositoryTests
{
    [Fact]
    public async Task GameRepository_SupportsQueriesAndWrites()
    {
        await using var context = TestData.CreateContext();
        var repository = new GameRepository(context);
        var game = await repository.AddAsync(TestData.Game(name: "Added", price: 17m), CancellationToken.None);

        Assert.True(game.Id > 0);
        Assert.Equal("Added", Assert.Single(await repository.GetAllAsync(CancellationToken.None)).Name);
        Assert.Equal(game.Id, (await repository.GetByIdAsync(game.Id, CancellationToken.None))!.Id);
        Assert.Equal([game.Id], (await repository.GetByIdsAsync([game.Id, 900], CancellationToken.None))
            .Select(x => x.Id));
        Assert.Equal(17m, await repository.GetPriceByIdAsync(game.Id, CancellationToken.None));
        Assert.Equal(0m, await repository.GetPriceByIdAsync(900, CancellationToken.None));

        game.Name = "Updated";
        await repository.UpdateAsync(game, CancellationToken.None);
        Assert.Equal("Updated", (await repository.GetByIdAsync(game.Id, CancellationToken.None))!.Name);

        await repository.DeleteAsync(game, CancellationToken.None);
        Assert.Empty(await repository.GetAllAsync(CancellationToken.None));
        Assert.Null(await repository.GetByIdAsync(game.Id, CancellationToken.None));
    }

    [Fact]
    public async Task PromotionRepository_SupportsQueriesAndWritesWithGames()
    {
        await using var context = TestData.CreateContext();
        var repository = new PromotionRepository(context);
        var game = TestData.Game();
        await context.Games.AddAsync(game);
        await context.SaveChangesAsync();

        var promotion = TestData.Promotion(name: "Added", games: [game]);
        await repository.AddAsync(promotion, CancellationToken.None);
        Assert.Equal("Added", Assert.Single(await repository.GetAllAsync(CancellationToken.None)).Name);
        Assert.Equal(game.Id, (await repository.GetByIdAsync(promotion.Id, CancellationToken.None))!
            .Games.Single().Id);

        promotion.Name = "Updated";
        await repository.UpdateAsync(promotion, CancellationToken.None);
        Assert.Equal("Updated", (await repository.GetByIdAsync(promotion.Id, CancellationToken.None))!.Name);

        await repository.DeleteAsync(promotion, CancellationToken.None);
        Assert.Empty(await repository.GetAllAsync(CancellationToken.None));
        Assert.Null(await repository.GetByIdAsync(promotion.Id, CancellationToken.None));
    }

    [Fact]
    public async Task UserGameRepository_FiltersAndPersistsLibraryEntries()
    {
        await using var context = TestData.CreateContext();
        var repository = new UserGameRepository(context);
        var game = TestData.Game();
        await context.Games.AddAsync(game);
        await context.SaveChangesAsync();

        var entry = await repository.AddAsync(new UserGame { UserId = 1, GameId = game.Id }, CancellationToken.None);

        Assert.True(entry.Id > 0);
        Assert.NotNull(await repository.GetByUserAndGameAsync(1, game.Id, CancellationToken.None));
        Assert.Null(await repository.GetByUserAndGameAsync(2, game.Id, CancellationToken.None));
        Assert.Null(await repository.GetByUserAndGameAsync(1, 999, CancellationToken.None));
        Assert.Single(await repository.GetByUserIdAsync(1, CancellationToken.None));
        Assert.Empty(await repository.GetByUserIdAsync(2, CancellationToken.None));
    }

    [Fact]
    public async Task DbInitializer_SeedsGamesWithoutAddingLibraryEntries()
    {
        await using var context = TestData.CreateContext();

        await DbInitializer.InitialyzeCatalogAsync(context);

        Assert.Equal(16, context.Games.Count());
        Assert.Equal(14, context.UserGames.Count());

        await DbInitializer.InitialyzeCatalogAsync(context);

        Assert.Equal(16, context.Games.Count());
        Assert.Equal(14, context.UserGames.Count());
    }

    [Fact]
    public async Task DbInitializer_DoesNotReplaceExistingGamesOrSeedLibrary()
    {
        await using var context = TestData.CreateContext();
        await context.Games.AddAsync(TestData.Game(name: "Existing"));
        await context.SaveChangesAsync();

        await DbInitializer.InitialyzeCatalogAsync(context);

        Assert.Equal("Existing", Assert.Single(context.Games).Name);
        Assert.Equal(14, context.UserGames.Count());
    }
}
