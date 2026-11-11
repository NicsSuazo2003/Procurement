
using Drmc.Procurement.Domain.Entities.APP;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Drmc.Procurement.Infrastructure.Persistence.Configurations;

public class APPItemConfiguration : IEntityTypeConfiguration<APPItem>
{
    public void Configure(EntityTypeBuilder<APPItem> builder)
    {
        builder.ToTable("APPItems");

        builder.HasKey(i => i.APPItemId);

        builder.Property(i => i.ItemCode)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(i => i.ItemDescription)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(i => i.ItemType)
            .HasMaxLength(100);

        builder.Property(i => i.Unit)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(i => i.Specifications)
            .HasMaxLength(1000);

        builder.Property(i => i.TotalQuantity)
            .HasPrecision(18, 2);

        builder.Property(i => i.EstimatedUnitPrice)
            .HasPrecision(18, 2);

        builder.Property(i => i.EstimatedBudget)
            .HasPrecision(18, 2);

        builder.HasOne(i => i.APPCategory)
            .WithMany(c => c.Items)
            .HasForeignKey(i => i.APPCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(i => i.Item)
            .WithMany()
            .HasForeignKey(i => i.ItemId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
