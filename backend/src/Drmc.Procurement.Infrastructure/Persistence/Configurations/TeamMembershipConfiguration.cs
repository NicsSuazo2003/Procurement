using Drmc.Procurement.Domain.Entities.Organization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Drmc.Procurement.Infrastructure.Persistence.Configurations;

public class TeamMembershipConfiguration : IEntityTypeConfiguration<TeamMembership>
{
    public void Configure(EntityTypeBuilder<TeamMembership> builder)
    {
        builder.ToTable("TeamMemberships");
        builder.HasKey(membership => membership.TeamMembershipId);
        builder.HasIndex(membership => new { membership.TeamId, membership.UserId }).IsUnique();
        builder.HasOne(membership => membership.Team)
            .WithMany(team => team.Memberships)
            .HasForeignKey(membership => membership.TeamId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(membership => membership.User)
            .WithMany()
            .HasForeignKey(membership => membership.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}