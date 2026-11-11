
using Drmc.Procurement.Domain.Entities.Procurement;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Drmc.Procurement.Infrastructure.Persistence.Configurations;

public class PPMPSectionAllocationConfiguration
    : IEntityTypeConfiguration<PPMPSectionAllocation>
{
    public void Configure(EntityTypeBuilder<PPMPSectionAllocation> builder)
    {
        builder.ToTable("PPMPSectionAllocations");

        builder.HasKey(a => a.AllocationId);

        builder.Property(a => a.Quantity)
            .HasPrecision(18, 2);

        builder.HasOne(a => a.PPMPItem)
            .WithMany(i => i.SectionAllocations)
            .HasForeignKey(a => a.PPMPItemId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.Section)
            .WithMany()
            .HasForeignKey(a => a.SectionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(a => new
        {
            a.PPMPItemId,
            a.SectionId
        }).IsUnique();
    }
}
