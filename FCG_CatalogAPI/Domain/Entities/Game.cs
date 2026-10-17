namespace FCG_CatalogAPI.Domain.Entities;


/// <summary>
/// Summary description for Class1
/// </summary>
public sealed record Game
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public decimal Price { get; set; }
}