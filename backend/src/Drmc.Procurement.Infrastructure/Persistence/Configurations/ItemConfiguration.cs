using Drmc.Procurement.Domain.Entities.Procurement;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Drmc.Procurement.Infrastructure.Persistence.Configurations;

public class ItemConfiguration : IEntityTypeConfiguration<Item>
{
    public void Configure(EntityTypeBuilder<Item> builder)
    {
        builder.ToTable("Items");

        builder.HasKey(i => i.ItemId);

        builder.Property(i => i.ItemCode)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(i => i.ItemName)
            .HasMaxLength(1500)
            .IsRequired();

        builder.Property(i => i.ItemType)
            .HasMaxLength(100);

        builder.Property(i => i.Unit)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(i => i.Specifications)
            .HasMaxLength(1500);

        builder.Property(i => i.IsActive)
            .HasDefaultValue(true);

        // Kept from before — used by ItemService.CreateAsync's
        // highest-suffix lookup and guarantees code uniqueness
        // within a category.
        builder.HasIndex(i => new
        {
            i.CategoryId,
            i.ItemCode
        })
        .IsUnique();

        // Active-items-by-category filter used by
        // ItemService.GetByCategoryIdAsync and the
        // GetByPlanningYearAndCategoryAsync join.
        builder.HasIndex(i => new { i.CategoryId, i.IsActive });
    }
}