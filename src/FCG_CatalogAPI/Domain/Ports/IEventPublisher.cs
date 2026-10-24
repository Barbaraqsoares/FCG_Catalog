using FCG_CatalogAPI.Domain.Events;

namespace FCG_CatalogAPI.Domain.Ports;
public interface IEventPublisher
{
    Task Publish(OrderPlacedCommand orderEvent, CancellationToken cancellationToken = default);
}