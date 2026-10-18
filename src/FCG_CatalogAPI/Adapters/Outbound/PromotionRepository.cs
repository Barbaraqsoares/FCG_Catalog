using FCG_CatalogAPI.Domain.Entities;
using FCG_CatalogAPI.Domain.Ports;
using FCG_CatalogAPI.Infrastructure.SqlServer;
using Microsoft.EntityFrameworkCore;

namespace FCG_CatalogAPI.Adapters.Outbound;

public class PromotionRepository : IPromotionRepository
{
    private readonly CatalogDbContext _context;

    public PromotionRepository(CatalogDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Promotion>> GetAllAsync()
    {
        return await _context.Promotions.Include(promotion => promotion.Games).ToListAsync();
    }

    public async Task<Promotion?> GetByIdAsync(int id)
    {
        return await _context.Promotions.Include(promotion => promotion.Games).FirstOrDefaultAsync(promotion => promotion.Id == id);
    }

    public async Task<Promotion> AddAsync(Promotion promotion)
    {
        await _context.Promotions.AddAsync(promotion);
        await _context.SaveChangesAsync();
        return promotion;
    }

    public async Task UpdateAsync(Promotion promotion)
    {
        _context.Promotions.Update(promotion);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Promotion promotion)
    {
        _context.Promotions.Remove(promotion);
        await _context.SaveChangesAsync();
    }
}