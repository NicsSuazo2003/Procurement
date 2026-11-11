
using Drmc.Procurement.Domain.Entities.Audit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Drmc.Procurement.Infrastructure.Persistence.Configurations;

public class PPMPChangeHistoryConfiguration
    : IEntityTypeConfiguration<PPMPChangeHistory>
{
    public void Configure(EntityTypeBuilder<PPMPChangeHistory> builder)
    {
        builder.ToTable("PPMPChangeHistories");

        builder.HasKey(h => h.ChangeHistoryId);

        builder.Property(h => h.Reason)
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(h => h.PreviousBudget)
            .HasPrecision(18, 2);

        builder.Property(h => h.UpdatedBudget)
            .HasPrecision(18, 2);

        builder.Property(h => h.ChangedAt)
            .IsRequired();

        builder.HasOne(h => h.PPMP)
            .WithMany()
            .HasForeignKey(h => h.PPMPId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(h => h.ChangedByUser)
            .WithMany()
            .HasForeignKey(h => h.ChangedBy)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
