using Drmc.Procurement.Domain.Entities.Procurement;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Drmc.Procurement.Infrastructure.Persistence.Configurations;

public class PPMPConfiguration : IEntityTypeConfiguration<PPMP>
{
    public void Configure(EntityTypeBuilder<PPMP> builder)
    {
        builder.ToTable("PPMPs");

        builder.HasKey(p => p.PPMPId);

        builder.Property(p => p.PPMPReference)
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(p => p.PPMPReference)
            .IsUnique();

        builder.Property(p => p.Status)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(p => p.EstimatedBudget)
            .HasPrecision(18, 2);

        builder.Property(p => p.CreatedAt)
            .IsRequired();

        builder.HasOne(p => p.Department)
            .WithMany()
            .HasForeignKey(p => p.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.PlanningYear)
            .WithMany()
            .HasForeignKey(p => p.PlanningYearId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Creator)
            .WithMany()
            .HasForeignKey(p => p.CreatedBy)
            .OnDelete(DeleteBehavior.Restrict);

        // ---- Optimisation indexes ----

        // Composite index for GetAllAsync(departmentId) and list views
        // that combine a department filter with a status filter.
        builder.HasIndex(p => new { p.DepartmentId, p.Status });

        // Filtered index for the two sidebar badge queries.
        // 99% of PPMPs are in other statuses (Reviewed, Included in APP,
        // Draft), so this index stays small (~KB) even at 10,000 rows.
        builder.HasIndex(p => p.Status)
            .HasFilter(
                "[Status] IN ('For Unit Head Approval', 'Approved by Unit Head')");

        // FK indexes — not created automatically by EF Core.
        builder.HasIndex(p => p.PlanningYearId);
        builder.HasIndex(p => p.CreatedBy);
    }
}