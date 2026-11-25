using Drmc.Procurement.Domain.Entities.Organization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Drmc.Procurement.Infrastructure.Persistence.Configurations;

public class TeamConfiguration : IEntityTypeConfiguration<Team>
{
    public void Configure(EntityTypeBuilder<Team> builder)
    {
        builder.ToTable("Teams");
        builder.HasKey(team => team.TeamId);
        builder.Property(team => team.TeamCode).HasMaxLength(50).IsRequired();
        builder.Property(team => team.TeamName).HasMaxLength(200).IsRequired();
        builder.HasIndex(team => new { team.DepartmentId, team.TeamCode }).IsUnique();
        builder.HasOne(team => team.Department)
            .WithMany()
            .HasForeignKey(team => team.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}