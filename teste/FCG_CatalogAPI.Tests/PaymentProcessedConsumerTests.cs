using FCG_CatalogAPI.Adapters.Inbound.Consumers;
using FCG_CatalogAPI.Domain.Events;
using FCG_CatalogAPI.Domain.Ports;
using MassTransit;
using Moq;

namespace FCG_CatalogAPI.Tests;

public class PaymentProcessedConsumerTests
{
    [Fact]
    public async Task Consume_DelegatesPaymentToUserGameUseCase()
    {
        var useCase = new Mock<IUserGameUseCase>();
        var payment = new PaymentProcessedEvent(
            Guid.NewGuid(), 5, 8, 22m, "Approved", DateTime.UtcNow, null);
        var context = new Mock<ConsumeContext<PaymentProcessedEvent>>();
        context.SetupGet(x => x.Message).Returns(payment);
        var consumer = new PaymentProcessedConsumer(useCase.Object);

        await consumer.Consume(context.Object);

        useCase.Verify(x => x.ProcessPaymentAsync(payment), Times.Once);
    }
}
