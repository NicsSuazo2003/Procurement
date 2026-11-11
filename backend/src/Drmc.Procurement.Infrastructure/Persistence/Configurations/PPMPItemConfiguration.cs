using Drmc.Procurement.Domain.Entities.Procurement;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Drmc.Procurement.Infrastructure.Persistence.Configurations;

public class PPMPItemConfiguration : IEntityTypeConfiguration<PPMPItem>
{
    public void Configure(EntityTypeBuilder<PPMPItem> builder)
    {
        builder.ToTable("PPMPItems");

        builder.HasKey(i => i.PPMPItemId);

        // Internal/catalogue information
        builder.Property(i => i.ItemCode)
            .HasMaxLength(100);

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

        // Official PPMP form fields
        builder.Property(i => i.GeneralDescriptionAndObjective)
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(i => i.ProjectType)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(i => i.PreProcurementConference)
            .IsRequired();

        // Quantity and ABC
        builder.Property(i => i.Quantity)
            .HasPrecision(18, 2);

        builder.Property(i => i.UnitPrice)
            .HasPrecision(18, 2);

        builder.Property(i => i.EstimatedBudget)
            .HasPrecision(18, 2);

        // PPMP relationship
        builder.HasOne(i => i.PPMP)
            .WithMany(p => p.Items)
            .HasForeignKey(i => i.PPMPId)
            .OnDelete(DeleteBehavior.Restrict);

        // Catalogue item relationship
        builder.HasOne(i => i.Item)
            .WithMany()
            .HasForeignKey(i => i.ItemId)
            .OnDelete(DeleteBehavior.Restrict);

        // Procurement category relationship
        builder.HasOne(i => i.Category)
            .WithMany()
            .HasForeignKey(i => i.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // ---- Optimisation indexes ----

        // Parent lookup — used by GetAllAsync's Items.Count() subquery
        // and by GetByIdAsync / GetChangeHistoryAsync.
        builder.HasIndex(i => i.PPMPId);

        // Category-grouping on the details page.
        builder.HasIndex(i => new { i.PPMPId, i.CategoryId });

        // FK indexes.
        builder.HasIndex(i => i.ItemId);
        builder.HasIndex(i => i.CategoryId);
    }
}