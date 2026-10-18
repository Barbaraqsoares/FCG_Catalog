using FCG_CatalogAPI.Adapters.Inbound.Responses;

namespace FCG_CatalogAPI.Domain.Ports;

public interface IUserGameUseCase
{
    Task<UserGameResponse> AddGameToLibraryAsync(int userId, int gameId);
    Task<List<UserGameResponse>> GetUserLibraryAsync(int userId);
}