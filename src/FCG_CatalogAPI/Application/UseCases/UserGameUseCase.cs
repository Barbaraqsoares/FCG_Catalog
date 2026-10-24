using FCG_CatalogAPI.Adapters.Inbound.Responses;
using FCG_CatalogAPI.Domain.Entities;
using FCG_CatalogAPI.Domain.Events;
using FCG_CatalogAPI.Domain.Exceptions;
using FCG_CatalogAPI.Domain.Ports;

namespace FCG_CatalogAPI.Application.UseCases;

public class UserGameUseCase : IUserGameUseCase
{
    private readonly IUserGameRepository _userGameRepository;
    private readonly IGameRepository _gameRepository;
    private readonly IEventPublisher _eventPublisher;

    public UserGameUseCase(IUserGameRepository userGameRepository, IGameRepository gameRepository, IEventPublisher eventPublisher)
    {
        _userGameRepository = userGameRepository;
        _gameRepository = gameRepository;
        _eventPublisher = eventPublisher;
    }

    public async Task<UserGameResponse> AddGameToLibraryAsync(int gameId)
    {
        return await AddGameToLibraryAsync(1, gameId, publishOrder: true);
    }

    public async Task ProcessPaymentAsync(PaymentProcessedEvent payment)
    {
        if (!string.Equals(payment.Status, "Approved", StringComparison.OrdinalIgnoreCase))
            return;

        var existingUserGame = await _userGameRepository.GetByUserAndGameAsync(payment.UserId, payment.GameId);
        if (existingUserGame is not null)
            return;

        await AddGameToLibraryAsync(payment.UserId, payment.GameId, payment.ProcessedAtUtc, publishOrder: false);
    }

    private async Task<UserGameResponse> AddGameToLibraryAsync(
        int userId,
        int gameId,
        DateTime? purchasedAtUtc = null,
        bool publishOrder = true)
    {
        var game = await _gameRepository.GetByIdAsync(gameId);

        if (game == null)
            throw new NotFoundException("Jogo não encontrado.");

        var existingUserGame = await _userGameRepository.GetByUserAndGameAsync(userId, gameId);

        if (existingUserGame != null)
            throw new ConflictException("O jogo já está na biblioteca do usuário.");

        var userGame = new UserGame
        {
            UserId = userId,
            GameId = gameId,
            PurchasedAt = purchasedAtUtc ?? DateTime.Now
        };

        var createdUserGame = await _userGameRepository.AddAsync(userGame);

        if (publishOrder)
        {
            var gamePrice = await _gameRepository.GetPriceByIdAsync(gameId);
            var orderEvent = new OrderPlacedCommand(userGame.UserId, gameId, gamePrice);
            await _eventPublisher.Publish(orderEvent);
        }

        return new UserGameResponse
        {
            GameId = game.Id,
            GameName = game.Name,
            Price = game.Price,
            PurchasedAt = createdUserGame.PurchasedAt
        };
    }

    public async Task<List<UserGameResponse>> GetUserLibraryAsync(int userId)
    {
        var userGames = await _userGameRepository.GetByUserIdAsync(userId);

        return userGames.Select(userGame => new UserGameResponse()
        {
            GameId = userGame.GameId,
            GameName = userGame.Game.Name,
            Price = userGame.Game.Price,
            PurchasedAt = userGame.PurchasedAt
        }).ToList();
    }
}