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

    public async Task<UserGameResponse> AddGameToLibraryAsync(int gameId, int userId, CancellationToken cancellationToken)
    {
        userId = 1; // Hardcoded userId for demonstration purposes
        var game = await _gameRepository.GetByIdAsync(gameId, cancellationToken);

        if (game == null)
            throw new NotFoundException("Jogo não encontrado.");

        var existingUserGame = await _userGameRepository.GetByUserAndGameAsync(userId, gameId, cancellationToken);
        if (existingUserGame is not null)
            throw new ConflictException("O jogo já está na biblioteca do usuário.");

        var gamePrice = await _gameRepository.GetPriceByIdAsync(gameId, cancellationToken);

        await _eventPublisher.Publish(new OrderPlacedCommand(userId, gameId, gamePrice));

        return new UserGameResponse
        {
            GameId = game.Id,
            GameName = game.Name,
            Price = game.Price
        };
    }

    public async Task ProcessPaymentAsync(PaymentProcessedEvent payment, CancellationToken cancellationToken)
    {
        if (!string.Equals(payment.Status, "Approved", StringComparison.OrdinalIgnoreCase))
            throw new NotFoundException("Pagamento não aprovado.");

        var existingUserGame = await _userGameRepository.GetByUserAndGameAsync(payment.UserId, payment.GameId, cancellationToken);
        if (existingUserGame is not null)
            return;

        var game = await _gameRepository.GetByIdAsync(payment.GameId, cancellationToken);
        if (game == null)
            throw new NotFoundException("Jogo não encontrado.");

        await _userGameRepository.AddAsync(new UserGame
        {
            UserId = payment.UserId,
            GameId = payment.GameId,
            PurchasedAt = DateTime.Parse(payment.ProcessedAtUtc)
        }, cancellationToken); 
    }

    public async Task<List<UserGameResponse>> GetUserLibraryAsync(int userId, CancellationToken cancellationToken)
    {
        var userGames = await _userGameRepository.GetByUserIdAsync(userId, cancellationToken);

        return userGames.Select(userGame => new UserGameResponse()
        {
            GameId = userGame.GameId,
            GameName = userGame.Game.Name,
            Price = userGame.Game.Price,
            PurchasedAt = userGame.PurchasedAt
        }).ToList();
    }
}