
using Drmc.Procurement.Domain.Entities.Planning;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Drmc.Procurement.Infrastructure.Persistence.Configurations;

public class PlanningYearConfiguration
    : IEntityTypeConfiguration<PlanningYear>
{
    public void Configure(EntityTypeBuilder<PlanningYear> builder)
    {
        builder.ToTable("PlanningYears");

        builder.HasKey(p => p.PlanningYearId);

        builder.Property(p => p.Year)
            .IsRequired();

        builder.HasIndex(p => p.Year)
            .IsUnique();

        builder.Property(p => p.Status)
            .HasMaxLength(30)
            .IsRequired();

        builder.HasMany(p => p.Categories)
            .WithOne(pc => pc.PlanningYear)
            .HasForeignKey(pc => pc.PlanningYearId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
