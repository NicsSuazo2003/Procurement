
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
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(i => i.ItemType)
            .HasMaxLength(100);

        builder.Property(i => i.Unit)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(i => i.Specifications)
            .HasMaxLength(1000);

        builder.Property(i => i.PAPCode).HasMaxLength(100);
        builder.Property(i => i.ObjectCode).HasMaxLength(100);
        builder.Property(i => i.ProcurementProjectName).HasMaxLength(2000);
        builder.Property(i => i.GeneralDescription).HasMaxLength(4000);
        builder.Property(i => i.EndUser).HasMaxLength(500);
        builder.Property(i => i.Classification).HasMaxLength(100);
        builder.Property(i => i.ModeOfProcurement).HasMaxLength(500);
        builder.Property(i => i.BidEvaluationCriteria).HasMaxLength(100);
        builder.Property(i => i.TargetPRSubmissionDate).HasMaxLength(50);
        builder.Property(i => i.StartOfProcurementActivity).HasMaxLength(100);
        builder.Property(i => i.EndOfProcurementActivity).HasMaxLength(100);
        builder.Property(i => i.SourceOfFunds).HasMaxLength(500);
        builder.Property(i => i.ProcurementStrategyTools).HasMaxLength(1000);
        builder.Property(i => i.ProspectiveBidders).HasMaxLength(2000);
        builder.Property(i => i.PotentialObservers).HasMaxLength(2000);
        builder.Property(i => i.ProcurementAgent).HasMaxLength(2000);
        builder.Property(i => i.CollaboratingEntities).HasMaxLength(2000);
        builder.Property(i => i.Remarks).HasMaxLength(4000);

        builder.Property(i => i.TotalQuantity)
            .HasPrecision(18, 2);

        builder.Property(i => i.EstimatedUnitPrice)
            .HasPrecision(18, 2);

        builder.Property(i => i.EstimatedBudget)
            .HasPrecision(18, 2);

        builder.Property(i => i.MOOE).HasPrecision(18, 2);
        builder.Property(i => i.CapitalOutlay).HasPrecision(18, 2);
        builder.Property(i => i.MultiYearContractCost).HasPrecision(18, 2);
        builder.Property(i => i.LifeCycleCost).HasPrecision(18, 2);

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
