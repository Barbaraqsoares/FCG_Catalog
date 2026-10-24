namespace FCG_CatalogAPI.Domain.Events;

public record OrderPlacedCommand(int UserId, int GameId, decimal Price);