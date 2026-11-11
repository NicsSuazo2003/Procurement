using Drmc.Procurement.Domain.Entities.Planning;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Drmc.Procurement.Infrastructure.Persistence.Configurations;

public class PlanningYearItemConfiguration
    : IEntityTypeConfiguration<PlanningYearItem>
{
    public void Configure(EntityTypeBuilder<PlanningYearItem> builder)
    {
        builder.ToTable("PlanningYearItems");

        builder.HasKey(x => x.PlanningYearItemId);

        builder.Property(x => x.ABCPerUnit)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.IsActive)
            .HasDefaultValue(true);

        builder.HasOne(x => x.PlanningYearCategory)
            .WithMany(x => x.Items)
            .HasForeignKey(x => x.PlanningYearCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Item)
            .WithMany()
            .HasForeignKey(x => x.ItemId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new
        {
            x.PlanningYearCategoryId,
            x.ItemId
        }).IsUnique();
    }
}