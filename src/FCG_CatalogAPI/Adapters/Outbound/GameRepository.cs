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

    public async Task<IEnumerable<Game>> GetAllAsync()
    {
        return await _context.Games.ToListAsync();
    }

    public async Task<Game?> GetByIdAsync(int id)
    {
        return await _context.Games
            .FirstOrDefaultAsync(game => game.Id == id);
    }

    public async Task<Game> AddAsync(Game game)
    {
        await _context.Games.AddAsync(game);

        await _context.SaveChangesAsync();

        return game;
    }

    public async Task UpdateAsync(Game game)
    {
        _context.Games.Update(game);

        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Game game)
    {
        _context.Games.Remove(game);

        await _context.SaveChangesAsync();
    }
    public async Task<List<Game>> GetByIdsAsync(IEnumerable<int> ids)
    {
        return await _context.Games.Where(game => ids.Contains(game.Id)).ToListAsync();
    }
}