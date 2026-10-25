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

    public async Task<UserGame?> GetByUserAndGameAsync(int userId, int gameId, CancellationToken cancellationToken)
    {
        var userGame = await _context.UserGames.FirstOrDefaultAsync(ug => ug.UserId == userId && ug.GameId == gameId, cancellationToken);
        return userGame;
    }

    public async Task<List<UserGame>> GetByUserIdAsync(int userId, CancellationToken cancellationToken)
    {
        var userGames = await _context.UserGames.Include(ug => ug.Game).Where(ug => ug.UserId == userId).ToListAsync(cancellationToken);
        return userGames;
    }

    public async Task<UserGame> AddAsync(UserGame userGame, CancellationToken cancellationToken)
    {
        _context.UserGames.Add(userGame);
        await _context.SaveChangesAsync();

        return userGame;
    }
}