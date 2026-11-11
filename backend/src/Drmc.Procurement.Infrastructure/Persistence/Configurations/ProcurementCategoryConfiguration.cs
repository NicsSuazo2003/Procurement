
using Drmc.Procurement.Domain.Entities.Procurement;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Drmc.Procurement.Infrastructure.Persistence.Configurations;

public class ProcurementCategoryConfiguration
    : IEntityTypeConfiguration<ProcurementCategory>
{
    public void Configure(EntityTypeBuilder<ProcurementCategory> builder)
    {
        builder.ToTable("ProcurementCategories");

        builder.HasKey(c => c.CategoryId);

        builder.Property(c => c.CategoryCode)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(c => c.CategoryName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(c => c.Description)
            .HasMaxLength(500);

        builder.Property(c => c.IsActive)
            .HasDefaultValue(true);

        builder.HasIndex(c => c.CategoryCode)
            .IsUnique();

        builder.HasMany(c => c.Items)
            .WithOne(i => i.Category)
            .HasForeignKey(i => i.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
