
using Drmc.Procurement.Domain.Entities.APP;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Drmc.Procurement.Infrastructure.Persistence.Configurations;

public class APPCategoryConfiguration
    : IEntityTypeConfiguration<APPCategory>
{
    public void Configure(EntityTypeBuilder<APPCategory> builder)
    {
        builder.ToTable("APPCategories");

        builder.HasKey(c => c.APPCategoryId);

        builder.Property(c => c.TotalEstimatedBudget)
            .HasPrecision(18, 2);

        builder.HasOne(c => c.APP)
            .WithMany(a => a.Categories)
            .HasForeignKey(c => c.APPId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.Category)
            .WithMany()
            .HasForeignKey(c => c.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(c => new
        {
            c.APPId,
            c.CategoryId
        }).IsUnique();
    }
}
