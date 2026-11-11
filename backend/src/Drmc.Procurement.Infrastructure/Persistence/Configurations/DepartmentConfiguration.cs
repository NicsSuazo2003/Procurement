using Drmc.Procurement.Domain.Entities.Organization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Drmc.Procurement.Infrastructure.Persistence.Configurations;

public class DepartmentConfiguration
    : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> builder)
    {
        builder.ToTable("Departments");

        builder.HasKey(d => d.DepartmentId);

        builder.Property(d => d.DepartmentCode)
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(d => d.DepartmentName)
            .HasMaxLength(150)
            .IsRequired();

        builder.HasIndex(d => d.DepartmentCode)
            .IsUnique();

        builder.Property(d => d.IsActive)
            .HasDefaultValue(true);

        builder.HasMany(d => d.Sections)
            .WithOne(s => s.Department)
            .HasForeignKey(s => s.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}