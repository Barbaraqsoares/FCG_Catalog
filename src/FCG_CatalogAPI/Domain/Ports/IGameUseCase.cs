using FCG_CatalogAPI.Adapters.Inbound.Requests;
using FCG_CatalogAPI.Adapters.Inbound.Responses;
using FCG_CatalogAPI.Domain.Exceptions;

namespace FCG_CatalogAPI.Domain.Ports;

public interface IGameUseCase
{
    Task<IEnumerable<GameResponse>> GetAllAsync();

    /// <summary>Lança <see cref="NotFoundException"/> quando o jogo não existe.</summary>
    Task<GameResponse> GetByIdAsync(int id);

    Task<GameResponse> CreateAsync(CreateGameRequest request);

    /// <summary>Lança <see cref="NotFoundException"/> quando o jogo não existe.</summary>
    Task<GameResponse> UpdateAsync(int id, UpdateGameRequest request);

    /// <summary>Lança <see cref="NotFoundException"/> quando o jogo não existe.</summary>
    Task DeleteAsync(int id);
}