using FCG_CatalogAPI.Domain.Entities;

namespace FCG_CatalogAPI.Domain.Ports;

public interface IUserGameRepository
{
    Task<UserGame?> GetByUserAndGameAsync(int userId, int gameId, CancellationToken cancellationToken);
    Task<List<UserGame>> GetByUserIdAsync(int userId, CancellationToken cancellationToken);
    Task<UserGame> AddAsync(UserGame userGame, CancellationToken cancellationToken);
}
