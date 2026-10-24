using FCG_CatalogAPI.Domain.Events;
using FCG_CatalogAPI.Domain.Ports;
using MassTransit;

namespace FCG_CatalogAPI.Adapters.Inbound.Consumers;

public sealed class PaymentProcessedConsumer : IConsumer<PaymentProcessedEvent>
{
    private readonly IUserGameUseCase _userGameUseCase;

    public PaymentProcessedConsumer(IUserGameUseCase userGameUseCase)
    {
        _userGameUseCase = userGameUseCase;
    }

    public Task Consume(ConsumeContext<PaymentProcessedEvent> context)
    {
        return _userGameUseCase.ProcessPaymentAsync(context.Message, context.CancellationToken);
    }
}
