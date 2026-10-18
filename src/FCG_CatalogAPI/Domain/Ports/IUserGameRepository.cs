using FCG_CatalogAPI.Domain.Entities;

namespace FCG_CatalogAPI.Domain.Ports;

public interface IUserGameRepository
{
    Task<UserGame?> GetByUserAndGameAsync(int userId, int gameId);
    Task<List<UserGame>> GetByUserIdAsync(int userId);
    Task<UserGame> AddAsync(UserGame userGame);
}
