using MassTransit;

namespace FCG_CatalogAPI.Domain.Events;

[EntityName("FCG_CatalogAPI:OrderPlacedEvent")]
public record OrderPlacedCommand(Guid OrderId, int UserId, int GameId, decimal Price);