
using Drmc.Procurement.Domain.Entities.APP;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Drmc.Procurement.Infrastructure.Persistence.Configurations;

public class APPItemSourceConfiguration
    : IEntityTypeConfiguration<APPItemSource>
{
    public void Configure(EntityTypeBuilder<APPItemSource> builder)
    {
        builder.ToTable("APPItemSources");

        builder.HasKey(s => s.APPItemSourceId);

        builder.Property(s => s.ContributedQuantity)
            .HasPrecision(18, 2);

        builder.Property(s => s.ContributedBudget)
            .HasPrecision(18, 2);

        builder.HasOne(s => s.APPItem)
            .WithMany(i => i.Sources)
            .HasForeignKey(s => s.APPItemId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.PPMPItem)
            .WithMany()
            .HasForeignKey(s => s.PPMPItemId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(s => new
        {
            s.APPItemId,
            s.PPMPItemId
        }).IsUnique();
    }
}
