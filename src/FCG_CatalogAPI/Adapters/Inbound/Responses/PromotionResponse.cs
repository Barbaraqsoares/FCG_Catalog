namespace FCG_CatalogAPI.Adapters.Inbound.Responses;
public record PromotionResponse
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public decimal Discount { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; }
    public int CreatedByUserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<int> GameIds { get; set; } = new();
}