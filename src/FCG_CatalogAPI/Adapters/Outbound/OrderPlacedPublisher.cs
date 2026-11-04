using FCG_CatalogAPI.Domain.Events;
using FCG_CatalogAPI.Domain.Ports;
using MassTransit;

namespace FCG_CatalogAPI.Adapters.Outbound;

public class OrderPlacedPublisher : IEventPublisher
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<OrderPlacedPublisher> _logger;

    public OrderPlacedPublisher(IPublishEndpoint publishEndpoint, ILogger<OrderPlacedPublisher> logger)
    {
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task Publish(OrderPlacedCommand orderEvent, CancellationToken cancellationToken = default)
    {
        try
        {
            await _publishEndpoint.Publish(orderEvent, cancellationToken);
            _logger.LogInformation("Evento {EventType} publicado, com o orderID {OrderId}.", typeof(OrderPlacedCommand).Name, orderEvent.OrderId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao publicar evento {EventType}, com o orderID {OrderId}.", typeof(OrderPlacedCommand).Name, orderEvent.OrderId);
            throw;
        }
    }
}