using FCG_CatalogAPI.Domain.Entities;
using FCG_CatalogAPI.Domain.Ports;
using FCG_CatalogAPI.Infrastructure.SqlServer;
using Microsoft.EntityFrameworkCore;

namespace FCG_CatalogAPI.Adapters.Outbound;

public class UserGameRepository : IUserGameRepository
{
    private readonly CatalogDbContext _context;

    public UserGameRepository(CatalogDbContext context)
    {
        _context = context;
    }

    public async Task<UserGame?> GetByUserAndGameAsync(int userId, int gameId)
    {
        return await _context.UserGames.FirstOrDefaultAsync(ug => ug.UserId == userId && ug.GameId == gameId);
    }

    public async Task<List<UserGame>> GetByUserIdAsync(int userId)
    {
        return await _context.UserGames.Include(ug => ug.Game).Where(ug => ug.UserId == userId).AsNoTracking().ToListAsync();
    }

    public async Task<UserGame> AddAsync(UserGame userGame)
    {
        _context.UserGames.Add(userGame);
        await _context.SaveChangesAsync();

        return userGame;
    }
}