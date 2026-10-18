namespace FCG_CatalogAPI.Domain.Entities;


/// <summary>
/// Summary description for Class1
/// </summary>
public sealed class Game : EntityBase
{
    public required string Name { get; set; }
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public bool IsActive { get; set; }
    public bool IsMultiplayer { get; set; }
    public ICollection<Promotion> Promotions { get; set; } = new List<Promotion>();
}