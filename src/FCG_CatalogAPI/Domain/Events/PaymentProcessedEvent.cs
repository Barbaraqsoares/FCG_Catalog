namespace FCG_CatalogAPI.Domain.Events;

public record PaymentProcessedEvent(
    Guid OrderId,
    int UserId,
    int GameId,
    decimal Price,
    string Status,
    DateTime ProcessedAtUtc,
    string? Reason);