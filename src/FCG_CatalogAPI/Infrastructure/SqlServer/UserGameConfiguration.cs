using FCG_CatalogAPI.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FCG_CatalogAPI.Infrastructure.SqlServer;

public class UserGameConfiguration : IEntityTypeConfiguration<UserGame>
{
    public void Configure(EntityTypeBuilder<UserGame> builder)
    {
        builder.HasKey(ug => ug.Id);
        builder.HasOne(ug => ug.Game).WithMany().HasForeignKey(ug => ug.GameId).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(ug => new { ug.UserId, ug.GameId }).IsUnique();
    }
}