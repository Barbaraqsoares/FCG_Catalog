using MassTransit;

namespace FCG_CatalogAPI.Domain.Events;

[EntityName("FCG_UserApi:user-created-event")]
public record OrderPlacedCommand(Guid OrderId, int UserId, int GameId, decimal Price);