using MassTransit;

namespace FCG_CatalogAPI.Domain.Events;

[EntityName("PaymentPublisher:PaymentProcessedEvent")]
public record PaymentProcessedEvent(Guid OrderId, int UserId, int GameId, string Price, string Status, string ProcessedAtUtc, string? Reason);