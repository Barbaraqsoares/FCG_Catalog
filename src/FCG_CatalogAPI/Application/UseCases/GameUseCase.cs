using FCG_CatalogAPI.Adapters.Inbound.Requests;
using FCG_CatalogAPI.Adapters.Inbound.Responses;
using FCG_CatalogAPI.Domain.Entities;
using FCG_CatalogAPI.Domain.Exceptions;
using FCG_CatalogAPI.Domain.Ports;

namespace FCG_CatalogAPI.Application.UseCases;

public class GameUseCase : IGameUseCase
{
    private readonly IGameRepository _gameRepository;
    public GameService(IGameRepository gameRepository)
    {
        _gameRepository = gameRepository;
    }

    public async Task<IEnumerable<GameResponse>> GetAllAsync()
    {
        var games = await _gameRepository.GetAllAsync();

        return games.Select(MapToResponse);
    }

    public async Task<GameResponse> GetByIdAsync(int id)
    {
        var game = await _gameRepository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Jogo {id} não encontrado.");

        return MapToResponse(game);
    }

    public async Task<GameResponse> CreateAsync(CreateGameRequest request)
    {
        ValidateGame(request.Name, request.Price);

        var game = new Game
        {
            Name = request.Name.Trim(),
            Description = request.Description,
            Price = request.Price,
            IsMultiplayer = request.IsMultiplayer,
            IsActive = true,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };

        var createdGame = await _gameRepository.AddAsync(game);

        return MapToResponse(createdGame);
    }

    public async Task<GameResponse> UpdateAsync(int id, UpdateGameRequest request)
    {
        var existingGame = await _gameRepository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Jogo {id} não encontrado.");

        ValidateGame(request.Name, request.Price);

        existingGame.Name = request.Name.Trim();
        existingGame.Description = request.Description;
        existingGame.Price = request.Price;
        existingGame.IsMultiplayer = request.IsMultiplayer;
        existingGame.IsActive = request.IsActive;
        existingGame.UpdatedAt = DateTime.Now;

        await _gameRepository.UpdateAsync(existingGame);

        return MapToResponse(existingGame);
    }

    public async Task DeleteAsync(int id)
    {
        var game = await _gameRepository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Jogo {id} não encontrado.");

        await _gameRepository.DeleteAsync(game);
    }

    private static void ValidateGame(string name, decimal price)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("O nome do jogo é obrigatório.");

        if (price < 0)
            throw new DomainException("O preço do jogo não pode ser negativo.");
    }

    private static GameResponse MapToResponse(Game game)
    {
        return new GameResponse
        {
            Id = game.Id,
            Name = game.Name,
            Description = game.Description,
            Price = game.Price,
            IsActive = game.IsActive,
            IsMultiplayer = game.IsMultiplayer,
            CreatedAt = game.CreatedAt,
            UpdatedAt = game.UpdatedAt
        };
    }
}
