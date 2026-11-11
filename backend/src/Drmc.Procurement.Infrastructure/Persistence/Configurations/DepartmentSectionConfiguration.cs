using Drmc.Procurement.Domain.Entities.Organization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Drmc.Procurement.Infrastructure.Persistence.Configurations;

public class DepartmentSectionConfiguration
    : IEntityTypeConfiguration<DepartmentSection>
{
    public void Configure(EntityTypeBuilder<DepartmentSection> builder)
    {
        builder.ToTable("DepartmentSections");

        builder.HasKey(s => s.SectionId);

        builder.Property(s => s.SectionCode)
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(s => s.SectionName)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(s => s.IsActive)
            .HasDefaultValue(true);

        builder.HasIndex(s => new
        {
            s.DepartmentId,
            s.SectionCode
        }).IsUnique();
    }
}