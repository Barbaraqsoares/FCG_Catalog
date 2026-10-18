namespace FCG_CatalogAPI.Domain.Events;

public record OrderPlacedEvent(int UserId, int GameId, decimal Price);