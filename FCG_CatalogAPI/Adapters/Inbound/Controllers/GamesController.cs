using Microsoft.AspNetCore.Mvc;

namespace FCG_CatalogAPI.Adapters.Inbound.Controllers;

[Route("api/[controller]")]
[ApiController]
/// <summary>
/// CRUD operations for games.
/// </summary>
public class GamesController : ControllerBase
{
    private readonly IPublishEndpoint _publisher;

    public GamesController(IPublishEndpoint publisher)
    {
        _publisher = publisher;
    }

    [HttpPost("{gameId}/buy")]
    public async Task<IActionResult> BuyGame(Guid gameId, [FromBody] Guid userId)
    {
        // Publica evento de compra
        await _publisher.Publish(new OrderPlacedEvent(userId, gameId, 59.90m));
        return Accepted("Order placed");
    }
}