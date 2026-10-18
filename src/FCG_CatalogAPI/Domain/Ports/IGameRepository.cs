using FCG_CatalogAPI.Domain.Entities;
using System;

namespace FCG_CatalogAPI.Domain.Ports;

public interface IGameRepository
{
    Task<IEnumerable<Game>> GetAllAsync();
    Task<Game?> GetByIdAsync(int id);
    Task<Game> AddAsync(Game game);
    Task UpdateAsync(Game game);
    Task DeleteAsync(Game game);
    Task<List<Game>> GetByIdsAsync(IEnumerable<int> ids);
}
