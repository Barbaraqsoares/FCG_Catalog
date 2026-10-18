namespace FCG_CatalogAPI.Adapters.Inbound.Requests;

public record CreatePromotionRequest
{
    public required string Name { get; set; }
    public decimal Discount { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public List<int> GameIds { get; set; } = new();
}