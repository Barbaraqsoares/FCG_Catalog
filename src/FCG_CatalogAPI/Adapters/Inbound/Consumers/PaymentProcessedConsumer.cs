using FCG_CatalogAPI.Domain.Events;
using FCG_CatalogAPI.Domain.Ports;
using MassTransit;
using System.Runtime.CompilerServices;

namespace FCG_CatalogAPI.Adapters.Inbound.Consumers;

public sealed class PaymentProcessedConsumer : IConsumer<PaymentProcessedEvent>
{
    private readonly IUserGameUseCase _userGameUseCase;

    public PaymentProcessedConsumer(IUserGameUseCase userGameUseCase)
    {
        _userGameUseCase = userGameUseCase;
    }

    public async Task Consume(ConsumeContext<PaymentProcessedEvent> context)
    {
        var message = context.Message;
        await _userGameUseCase.ProcessPaymentAsync(message, context.CancellationToken);
    }
}