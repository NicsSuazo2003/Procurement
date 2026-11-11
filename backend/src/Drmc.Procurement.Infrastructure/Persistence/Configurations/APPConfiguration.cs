
using Drmc.Procurement.Domain.Entities.APP;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Drmc.Procurement.Infrastructure.Persistence.Configurations;

public class APPConfiguration : IEntityTypeConfiguration<APP>
{
    public void Configure(EntityTypeBuilder<APP> builder)
    {
        builder.ToTable("APPs");

        builder.HasKey(a => a.APPId);

        builder.Property(a => a.APPReference)
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(a => a.APPReference)
            .IsUnique();

        builder.Property(a => a.Status)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(a => a.CreatedAt)
            .IsRequired();

        builder.HasOne(a => a.PlanningYear)
            .WithMany()
            .HasForeignKey(a => a.PlanningYearId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.Creator)
            .WithMany()
            .HasForeignKey(a => a.CreatedBy)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
