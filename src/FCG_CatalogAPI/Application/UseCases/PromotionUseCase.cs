using FCG_CatalogAPI.Adapters.Inbound.Requests;
using FCG_CatalogAPI.Adapters.Inbound.Responses;
using FCG_CatalogAPI.Domain.Entities;
using FCG_CatalogAPI.Domain.Exceptions;
using FCG_CatalogAPI.Domain.Ports;

namespace FCG_CatalogAPI.Application.UseCases;

public class PromotionUseCase : IPromotionUseCase
{
    private readonly IPromotionRepository _promotionRepository;
    private readonly IGameRepository _gameRepository;

    public PromotionUseCase(IPromotionRepository promotionRepository, IGameRepository gameRepository)
    {
        _promotionRepository = promotionRepository;
        _gameRepository = gameRepository;
    }
    private static void ValidatePromotion(CreatePromotionRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new DomainException("O nome da promoção é obrigatório.");

        if (request.Discount <= 0 || request.Discount > 100)
            throw new DomainException("Desconto precisa ser maior que 0 e até 100.");

        if (request.StartDate >= request.EndDate)
            throw new DomainException("A data de inicio precisa ser menor que a data fim.");

        if (request.GameIds.Count == 0)
            throw new DomainException("Ao menos 1 game precisa ser selecionado.");
    }

    public async Task<PromotionResponse> CreateAsync(CreatePromotionRequest request, int adminUserId)
    {
        ValidatePromotion(request);

        var games = await _gameRepository.GetByIdsAsync(request.GameIds);

        if (games.Count != request.GameIds.Distinct().Count())
            throw new DomainException("Um ou mais games não foram encontrados.");

        var promotion = new Promotion
        {
            Name = request.Name.Trim(),
            Discount = request.Discount,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            IsActive = true,
            CreatedByUserId = adminUserId,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now,
            Games = games
        };

        var createdPromotion = await _promotionRepository.AddAsync(promotion);

        return MapToResponse(createdPromotion);
    }
    private static PromotionResponse MapToResponse(Promotion promotion)
    {
        return new PromotionResponse
        {
            Id = promotion.Id,
            Name = promotion.Name,
            Discount = promotion.Discount,
            StartDate = promotion.StartDate,
            EndDate = promotion.EndDate,
            IsActive = promotion.IsActive,
            CreatedByUserId = promotion.CreatedByUserId,
            CreatedAt = promotion.CreatedAt,
            GameIds = promotion.Games
                .Select(game => game.Id)
                .ToList()
        };
    }

    public async Task<IEnumerable<PromotionResponse>> GetAllAsync()
    {
        var promotions = await _promotionRepository.GetAllAsync();

        return promotions.Select(MapToResponse);
    }

    public async Task<PromotionResponse> GetByIdAsync(int id)
    {
        var promotion = await _promotionRepository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Promoção {id} não encontrada.");

        return MapToResponse(promotion);
    }

    public async Task DeleteAsync(int id)
    {
        var promotion = await _promotionRepository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Promoção {id} não encontrada.");

        await _promotionRepository.DeleteAsync(promotion);
    }
}
