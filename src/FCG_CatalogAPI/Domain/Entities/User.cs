namespace FCG_CatalogAPI.Domain.Entities;
public record User
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
}