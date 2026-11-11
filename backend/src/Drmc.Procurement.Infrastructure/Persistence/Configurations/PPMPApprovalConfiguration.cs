
using Drmc.Procurement.Domain.Entities.Procurement;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Drmc.Procurement.Infrastructure.Persistence.Configurations;

public class PPMPApprovalConfiguration
    : IEntityTypeConfiguration<PPMPApproval>
{
    public void Configure(EntityTypeBuilder<PPMPApproval> builder)
    {
        builder.ToTable("PPMPApprovals");

        builder.HasKey(a => a.ApprovalId);

        builder.Property(a => a.ApprovalType)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(a => a.Remarks)
            .HasMaxLength(1000);

        builder.Property(a => a.ApprovedAt)
            .IsRequired();

        builder.HasOne(a => a.PPMP)
            .WithMany(p => p.Approvals)
            .HasForeignKey(a => a.PPMPId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.Approver)
            .WithMany()
            .HasForeignKey(a => a.ApprovedBy)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
