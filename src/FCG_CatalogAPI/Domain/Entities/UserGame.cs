namespace FCG_CatalogAPI.Domain.Entities;

public class UserGame : EntityBase
{
    public int UserId { get; set; }
    public int GameId { get; set; }
    public Game Game { get; set; } = null!;

    public DateTime PurchasedAt { get; set; } = DateTime.Now;
}