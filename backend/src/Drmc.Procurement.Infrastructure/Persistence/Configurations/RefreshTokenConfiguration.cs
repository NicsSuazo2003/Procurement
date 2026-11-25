using Drmc.Procurement.Domain.Entities.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Drmc.Procurement.Infrastructure.Persistence.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens");
        builder.HasKey(token => token.RefreshTokenId);
        builder.Property(token => token.TokenHash)
            .HasMaxLength(64)
            .IsRequired();
        builder.HasIndex(token => token.TokenHash)
            .IsUnique();
        builder.Property(token => token.RowVersion)
            .IsRowVersion();
        builder.HasOne(token => token.User)
            .WithMany()
            .HasForeignKey(token => token.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
