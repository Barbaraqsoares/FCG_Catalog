using FCG_CatalogAPI.Domain.Entities;

namespace FCG_CatalogAPI.Domain.Ports;

public interface IGameRepository
{
    Task<IEnumerable<Game>> GetAllAsync(CancellationToken cancellationToken);
    Task<Game?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<Game> AddAsync(Game game, CancellationToken cancellationToken);
    Task UpdateAsync(Game game, CancellationToken cancellationToken);
    Task DeleteAsync(Game game, CancellationToken cancellationToken);
    Task<List<Game>> GetByIdsAsync(IEnumerable<int> ids, CancellationToken cancellationToken);
    Task<decimal> GetPriceByIdAsync(int gameId, CancellationToken cancellationToken);
}