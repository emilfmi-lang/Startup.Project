

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Startup_Project.Domain.Entities;

namespace Startup_Project.Persistance.Configurations;

public class PromoCodeUsageConfiguration : IEntityTypeConfiguration<PromoCodeUsage>
{
    public void Configure(EntityTypeBuilder<PromoCodeUsage> builder)
    {
        builder.HasKey(u => u.Id);
        builder.HasIndex(u => new { u.PromoCodeId, u.UserId }).IsUnique();
        builder.Property(u => u.AppliedDiscount).HasPrecision(18, 2);
        builder.Property(x => x.UsedAt).IsRequired();

        builder.HasOne(u => u.PromoCode)
            .WithMany(p => p.PromoCodeUsages)
            .HasForeignKey(u => u.PromoCodeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(u => u.User)
            .WithMany()
            .HasForeignKey(u => u.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
