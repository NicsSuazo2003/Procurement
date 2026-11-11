
using Drmc.Procurement.Domain.Entities.Planning;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Drmc.Procurement.Infrastructure.Persistence.Configurations;

public class PlanningYearCategoryConfiguration
    : IEntityTypeConfiguration<PlanningYearCategory>
{
    public void Configure(
        EntityTypeBuilder<PlanningYearCategory> builder)
    {
        builder.ToTable("PlanningYearCategories");

        builder.HasKey(pc => pc.PlanningYearCategoryId);

        builder.HasOne(pc => pc.Category)
            .WithMany(c => c.PlanningYears)
            .HasForeignKey(pc => pc.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(pc => new
        {
            pc.PlanningYearId,
            pc.CategoryId
        }).IsUnique();

        builder.Property(pc => pc.IsActive)
            .HasDefaultValue(true);
    }
}
