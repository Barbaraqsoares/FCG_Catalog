using FCG_CatalogAPI.Domain.Entities;

namespace FCG_CatalogAPI.Domain.Ports;

public interface IPromotionRepository
{
    Task<IEnumerable<Promotion>> GetAllAsync(CancellationToken cancellationToken);
    Task<Promotion?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<Promotion> AddAsync(Promotion Promotion, CancellationToken cancellationToken);
    Task UpdateAsync(Promotion Promotion, CancellationToken cancellationToken);
    Task DeleteAsync(Promotion Promotion, CancellationToken cancellationToken);
}
