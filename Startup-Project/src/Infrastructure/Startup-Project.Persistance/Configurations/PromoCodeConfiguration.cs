
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Startup_Project.Domain.Entities;

namespace Startup_Project.Persistance.Configurations;

public class PromoCodeConfiguration : IEntityTypeConfiguration<PromoCode>
{
    public void Configure(EntityTypeBuilder<PromoCode> builder)
    {
        builder.HasKey(x =>x.Id);
        builder.HasIndex(p => p.Code).IsUnique();

        builder.Property(p => p.Code).IsRequired().HasMaxLength(50);
        builder.Property(p => p.DiscountValue).HasPrecision(18, 2);
        builder.Property(p => p.MinimumOrderAmount).HasPrecision(18, 2);
        builder.Property(p => p.UsedCount).IsConcurrencyToken();

        builder.HasMany(p => p.PromoCodeUsages)
       .WithOne(u => u.PromoCode)
       .HasForeignKey(u => u.PromoCodeId)
       .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(p => p.Package)
            .WithMany()
            .HasForeignKey(p => p.PackageId)
            .OnDelete(DeleteBehavior.Cascade);





































































    }
}
