namespace FCG_CatalogAPI.Domain.Events;

public record PaymentProcessedEvent(Guid OrderId, int UserId, int GameId, string Price, string Status, DateTime ProcessedAtUtc, string? Reason);