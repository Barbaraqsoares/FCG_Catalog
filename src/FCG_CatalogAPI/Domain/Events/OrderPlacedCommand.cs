namespace FCG_CatalogAPI.Domain.Events;

public record OrderPlacedCommand(Guid OrderId, int UserId, int GameId, decimal Price);