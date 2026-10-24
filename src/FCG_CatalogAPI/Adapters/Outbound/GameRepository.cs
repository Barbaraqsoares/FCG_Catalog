using FCG_CatalogAPI.Domain.Entities;
using FCG_CatalogAPI.Domain.Ports;
using FCG_CatalogAPI.Infrastructure.SqlServer;
using Microsoft.EntityFrameworkCore;

namespace FCG_CatalogAPI.Adapters.Outbound;

public class GameRepository : IGameRepository
{
    private readonly CatalogDbContext _context;

    public GameRepository(CatalogDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Game>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await _context.Games.ToListAsync();
    }

    public async Task<Game?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        return await _context.Games
            .FirstOrDefaultAsync(game => game.Id == id);
    }

    public async Task<Game> AddAsync(Game game, CancellationToken cancellationToken)
    {
        await _context.Games.AddAsync(game);

        await _context.SaveChangesAsync();

        return game;
    }

    public async Task UpdateAsync(Game game, CancellationToken cancellationToken)
    {
        _context.Games.Update(game);

        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Game game, CancellationToken cancellationToken)
    {
        _context.Games.Remove(game);

        await _context.SaveChangesAsync();
    }
    public async Task<List<Game>> GetByIdsAsync(IEnumerable<int> ids, CancellationToken cancellationToken)
    {
        return await _context.Games.Where(game => ids.Contains(game.Id)).ToListAsync();
    }

    public async Task<decimal> GetPriceByIdAsync(int gameId, CancellationToken cancellationToken)
    {
        return await _context.Games
            .Where(game => game.Id == gameId)
            .Select(game => game.Price)
            .FirstOrDefaultAsync();
    }
}