namespace FCG_CatalogAPI.Adapters.Inbound.Responses;

public record UserGameResponse
{
    public int GameId { get; set; }
    public string GameName { get; set; }
    public decimal Price { get; set; }
    public DateTime PurchasedAt { get; set; }
}