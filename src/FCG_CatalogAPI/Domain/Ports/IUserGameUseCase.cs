using FCG_CatalogAPI.Adapters.Inbound.Responses;
using FCG_CatalogAPI.Domain.Events;

namespace FCG_CatalogAPI.Domain.Ports;

public interface IUserGameUseCase
{
    Task<UserGameResponse> AddGameToLibraryAsync(int gameId);
    Task ProcessPaymentAsync(PaymentProcessedEvent payment);
    Task<List<UserGameResponse>> GetUserLibraryAsync(int userId);
}