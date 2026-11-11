using Drmc.Procurement.Domain.Entities.Audit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Drmc.Procurement.Infrastructure.Persistence.Configurations;

public class PPMPChangeHistorySectionAllocationConfiguration
    : IEntityTypeConfiguration<PPMPChangeHistorySectionAllocation>
{
    public void Configure(
        EntityTypeBuilder<PPMPChangeHistorySectionAllocation> builder)
    {
        builder.ToTable("PPMPChangeHistorySectionAllocations");

        builder.HasKey(x => x.ChangeHistorySectionAllocationId);

        builder.Property(x => x.PreviousSectionCode)
            .HasMaxLength(50);

        builder.Property(x => x.PreviousSectionName)
            .HasMaxLength(150);

        builder.Property(x => x.PreviousQuantity)
            .HasPrecision(18, 2);

        builder.Property(x => x.UpdatedSectionCode)
            .HasMaxLength(50);

        builder.Property(x => x.UpdatedSectionName)
            .HasMaxLength(150);

        builder.Property(x => x.UpdatedQuantity)
            .HasPrecision(18, 2);

        builder.HasOne(x => x.ChangeHistoryItem)
            .WithMany(x => x.SectionAllocations)
            .HasForeignKey(x => x.ChangeHistoryItemId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}