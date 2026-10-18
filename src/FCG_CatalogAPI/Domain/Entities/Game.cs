namespace FCG_CatalogAPI.Domain.Entities;

public sealed class Game : EntityBase
{
    public required string Name { get; set; }
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public bool IsActive { get; set; }
    public bool IsMultiplayer { get; set; }
    public ICollection<Promotion> Promotions { get; set; } = new List<Promotion>();
}