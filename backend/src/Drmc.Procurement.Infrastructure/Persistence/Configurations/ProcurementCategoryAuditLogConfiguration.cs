using Drmc.Procurement.Domain.Entities.Audit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Drmc.Procurement.Infrastructure.Persistence.Configurations;

public class ProcurementCategoryAuditLogConfiguration
    : IEntityTypeConfiguration<ProcurementCategoryAuditLog>
{
    public void Configure(
        EntityTypeBuilder<ProcurementCategoryAuditLog> builder)
    {
        builder.ToTable("ProcurementCategoryAuditLogs");

        builder.HasKey(a => a.CategoryAuditLogId);

        builder.Property(a => a.Action)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(a => a.Reason)
            .HasMaxLength(1000);

        builder.Property(a => a.PreviousCategoryCode)
            .HasMaxLength(50);

        builder.Property(a => a.PreviousCategoryName)
            .HasMaxLength(200);

        builder.Property(a => a.PreviousDescription)
            .HasMaxLength(500);

        builder.Property(a => a.UpdatedCategoryCode)
            .HasMaxLength(50);

        builder.Property(a => a.UpdatedCategoryName)
            .HasMaxLength(200);

        builder.Property(a => a.UpdatedDescription)
            .HasMaxLength(500);

        builder.Property(a => a.ChangedAt)
            .IsRequired();

        // Category relationship
        builder.HasOne(a => a.Category)
            .WithMany(c => c.AuditLogs)
            .HasForeignKey(a => a.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // User who performed the change
        builder.HasOne(a => a.ChangedByUser)
            .WithMany()
            .HasForeignKey(a => a.ChangedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Useful for retrieving a category's history chronologically
        builder.HasIndex(a => new
        {
            a.CategoryId,
            a.ChangedAt
        });
    }
}