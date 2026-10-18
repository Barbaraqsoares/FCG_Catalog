namespace FCG_CatalogAPI.Adapters.Inbound.Requests;

public record UpdateGameRequest
{
    public required string Name { get; set; }
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public bool IsMultiplayer { get; set; }
    public bool IsActive { get; set; }
}