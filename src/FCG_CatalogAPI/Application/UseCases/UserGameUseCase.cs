using FCG_CatalogAPI.Adapters.Inbound.Responses;
using FCG_CatalogAPI.Domain.Entities;
using FCG_CatalogAPI.Domain.Exceptions;
using FCG_CatalogAPI.Domain.Ports;

namespace FCG_CatalogAPI.Application.UseCases;

public class UserGameUseCase : IUserGameUseCase
{
    private readonly IUserGameRepository _userGameRepository;
    private readonly IGameRepository _gameRepository;

    public UserGameUseCase(IUserGameRepository userGameRepository, IGameRepository gameRepository)
    {
        _userGameRepository = userGameRepository;
        _gameRepository = gameRepository;
    }

    public async Task<UserGameResponse> AddGameToLibraryAsync(int userId, int gameId)
    {
        var user = new User() { Id = 2, Name = "John Doe" }; // Simulação de busca do usuário 

        if (user == null)
            throw new NotFoundException("Usuário não encontrado.");

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
            PurchasedAt = DateTime.Now
        };

        var createdUserGame = await _userGameRepository.AddAsync(userGame);

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
        var user = new User() { Id = 2, Name = "John Doe" }; // Simulação de busca do usuário 

        if (user == null)
            throw new NotFoundException("Usuário não encontrado.");

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