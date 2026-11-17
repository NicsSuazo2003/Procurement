
using Drmc.Procurement.Domain.Entities.Audit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Drmc.Procurement.Infrastructure.Persistence.Configurations;

public class PPMPChangeHistoryItemConfiguration
    : IEntityTypeConfiguration<PPMPChangeHistoryItem>
{
    public void Configure(EntityTypeBuilder<PPMPChangeHistoryItem> builder)
    {
        builder.ToTable("PPMPChangeHistoryItems");

        builder.HasKey(i => i.ChangeHistoryItemId);

        // Previous item information
        builder.Property(i => i.PreviousItemCode)
            .HasMaxLength(100);

        builder.Property(i => i.PreviousDescription)
            .HasMaxLength(500);

        builder.Property(i => i.PreviousUnit)
            .HasMaxLength(50);

        builder.Property(i => i.PreviousQuantity)
            .HasPrecision(18, 2);

        builder.Property(i => i.PreviousUnitPrice)
            .HasPrecision(18, 2);

        builder.Property(i => i.PreviousEstimatedBudget)
            .HasPrecision(18, 2);

        // Previous official PPMP form fields
        builder.Property(i => i.PreviousGeneralDescriptionAndObjective)
            .HasMaxLength(2000);

        builder.Property(i => i.PreviousProjectType)
            .HasMaxLength(100);

        builder.Property(i => i.PreviousRecommendedModeOfProcurement)
            .HasMaxLength(150);

        builder.Property(i => i.PreviousStartOfProcurementActivity)
            .HasMaxLength(7);

        builder.Property(i => i.PreviousEndOfProcurementActivity)
            .HasMaxLength(7);

        builder.Property(i => i.PreviousExpectedDeliveryImplementationPeriod)
            .HasMaxLength(100);

        builder.Property(i => i.PreviousSourceOfFunds)
            .HasMaxLength(100);

        builder.Property(i => i.PreviousAttachedSupportingDocuments)
            .HasMaxLength(500);

        // PreviousPreProcurementConference is nullable bool.
        // No additional configuration is required.

        // Updated item information
        builder.Property(i => i.UpdatedItemCode)
            .HasMaxLength(100);

        builder.Property(i => i.UpdatedDescription)
            .HasMaxLength(500);

        builder.Property(i => i.UpdatedUnit)
            .HasMaxLength(50);

        builder.Property(i => i.UpdatedQuantity)
            .HasPrecision(18, 2);

        builder.Property(i => i.UpdatedUnitPrice)
            .HasPrecision(18, 2);

        builder.Property(i => i.UpdatedEstimatedBudget)
            .HasPrecision(18, 2);

        // Updated official PPMP form fields
        builder.Property(i => i.UpdatedGeneralDescriptionAndObjective)
            .HasMaxLength(2000);

        builder.Property(i => i.UpdatedProjectType)
            .HasMaxLength(100);

        builder.Property(i => i.UpdatedRecommendedModeOfProcurement)
            .HasMaxLength(150);

        builder.Property(i => i.UpdatedStartOfProcurementActivity)
            .HasMaxLength(7);

        builder.Property(i => i.UpdatedEndOfProcurementActivity)
            .HasMaxLength(7);

        builder.Property(i => i.UpdatedExpectedDeliveryImplementationPeriod)
            .HasMaxLength(100);

        builder.Property(i => i.UpdatedSourceOfFunds)
            .HasMaxLength(100);

        builder.Property(i => i.UpdatedAttachedSupportingDocuments)
            .HasMaxLength(500);

        // UpdatedPreProcurementConference is nullable bool.
        // No additional configuration is required.

        // Change history relationship
        builder.HasOne(i => i.ChangeHistory)
            .WithMany(h => h.Items)
            .HasForeignKey(i => i.ChangeHistoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
