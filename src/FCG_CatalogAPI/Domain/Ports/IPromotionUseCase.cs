using FCG_CatalogAPI.Adapters.Inbound.Requests;
using FCG_CatalogAPI.Adapters.Inbound.Responses;
using FCG_CatalogAPI.Domain.Exceptions;

namespace FCG_CatalogAPI.Domain.Ports;

public interface IPromotionUseCase
{
    Task<IEnumerable<PromotionResponse>> GetAllAsync();

    /// <summary>Lança <see cref="NotFoundException"/> quando a promoção não existe.</summary>
    Task<PromotionResponse> GetByIdAsync(int id);

    Task<PromotionResponse> CreateAsync(CreatePromotionRequest request, int adminUserId);

    /// <summary>Lança <see cref="NotFoundException"/> quando a promoção não existe.</summary>
    Task DeleteAsync(int id);
}