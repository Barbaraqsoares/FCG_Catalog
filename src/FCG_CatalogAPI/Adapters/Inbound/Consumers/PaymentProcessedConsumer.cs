
using FCG_CatalogAPI.Domain.Events;
using FCG_CatalogAPI.Domain.Ports;
using MassTransit;

namespace FCG_CatalogAPI.Adapters.Inbound.Consumers;

public sealed class PaymentProcessedConsumer
    : IConsumer<PaymentProcessedEvent>
{
    private readonly IUserGameUseCase _userGameUseCase;
    private readonly ILogger<PaymentProcessedConsumer> _logger;

    public PaymentProcessedConsumer(
        IUserGameUseCase userGameUseCase,
        ILogger<PaymentProcessedConsumer> logger)
    {
        _userGameUseCase = userGameUseCase;
        _logger = logger;
    }

    public async Task Consume(
        ConsumeContext<PaymentProcessedEvent> context)
    {
        var message = context.Message;

        _logger.LogInformation(
            "PaymentProcessedEvent recebido. OrderId: {OrderId}, Status: {Status}",
            message.OrderId,
            message.Status);

        await _userGameUseCase.ProcessPaymentAsync(
            message,
            context.CancellationToken);

        _logger.LogInformation(
            "PaymentProcessedEvent processado. OrderId: {OrderId}",
            message.OrderId);
    }
}
