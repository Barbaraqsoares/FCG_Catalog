using FCG_CatalogAPI.Domain.Events;
using FCG_CatalogAPI.Domain.Ports;
using MassTransit;

namespace FCG_CatalogAPI.Adapters.Outbound;

public class RabbitMqEventPublisher : IEventPublisher
{
    private readonly IPublishEndpoint _publishEndpoint;
    public RabbitMqEventPublisher(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }
    public async Task Publish(OrderPlacedCommand orderEvent, CancellationToken cancellationToken = default)
    {
        await _publishEndpoint.Publish(orderEvent, cancellationToken);
    }
}