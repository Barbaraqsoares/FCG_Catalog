using MassTransit;

namespace FCG_CatalogAPI.Domain.Events;

[EntityName("FCG_PaymentsAPI:PaymentProcessedEvent")]
public record PaymentProcessedEvent(Guid OrderId, int UserId, int GameId, decimal Price, string Status, DateTime ProcessedAtUtc, string? Reason);