using FCG_CatalogAPI.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FCG_CatalogAPI.Infrastructure.SqlServer;

public class PromotionConfiguration : IEntityTypeConfiguration<Promotion>
{
    public void Configure(EntityTypeBuilder<Promotion> builder)
    {
        builder.ToTable("Promotions")
           .HasMany(promotion => promotion.Games)
        .WithMany(game => game.Promotions)
        .UsingEntity(join => join.ToTable("GamePromotions"));

        builder.Property(p => p.Id).HasColumnType("int").UseIdentityColumn();
        builder.Property(p => p.Name).IsRequired().HasColumnType("nvarchar(100)");
        builder.Property(p => p.Discount).HasColumnType("decimal(18,2)");
        builder.Property(p => p.StartDate).HasColumnType("datetime2");
        builder.Property(p => p.EndDate).HasColumnType("datetime2");
        builder.Property(p => p.IsActive).HasColumnType("bit");
        builder.Property(p => p.CreatedByUserId).HasColumnType("int");
        builder.Property(p => p.CreatedAt).HasColumnType("datetime2");
        builder.Property(p => p.UpdatedAt).HasColumnType("datetime2");
    }
}