using FCG_CatalogAPI.Adapters.Inbound.Responses;
using FCG_CatalogAPI.Domain.Events;

namespace FCG_CatalogAPI.Domain.Ports;

public interface IUserGameUseCase
{
    Task<UserGameResponse> AddGameToLibraryAsync(int gameId, int userId, CancellationToken cancellationToken);
    Task ProcessPaymentAsync(PaymentProcessedEvent payment, CancellationToken cancellationToken);
    Task<List<UserGameResponse>> GetUserLibraryAsync(int userId, CancellationToken cancellationToken);
}