namespace FCG_CatalogAPI.Domain.Entities;

public class Promotion : EntityBase
{
    public required string Name { get; set; }
    public decimal Discount { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; }
    public int CreatedByUserId { get; set; }
    public ICollection<Game> Games { get; set; } = new List<Game>();
}