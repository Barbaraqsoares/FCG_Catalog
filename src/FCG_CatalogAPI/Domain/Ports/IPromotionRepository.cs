using FCG_CatalogAPI.Domain.Entities;

namespace FCG_CatalogAPI.Domain.Ports;

public interface IPromotionRepository
{
    Task<IEnumerable<Promotion>> GetAllAsync();
    Task<Promotion?> GetByIdAsync(int id);
    Task<Promotion> AddAsync(Promotion Promotion);
    Task UpdateAsync(Promotion Promotion);
    Task DeleteAsync(Promotion Promotion);
}
