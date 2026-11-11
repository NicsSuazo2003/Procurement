using Drmc.Procurement.Domain.Entities.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Drmc.Procurement.Infrastructure.Persistence.Configurations;

public class UserCategoryPreferenceConfiguration
    : IEntityTypeConfiguration<UserCategoryPreference>
{
    public void Configure(EntityTypeBuilder<UserCategoryPreference> builder)
    {
        builder.ToTable("UserCategoryPreferences");

        builder.HasKey(x => x.UserCategoryPreferenceId);

        builder.HasOne(x => x.User)
            .WithMany(x => x.CategoryPreferences)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.PlanningYearCategory)
            .WithMany(x => x.UserPreferences)
            .HasForeignKey(x => x.PlanningYearCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new
        {
            x.UserId,
            x.PlanningYearCategoryId
        }).IsUnique();
    }
}