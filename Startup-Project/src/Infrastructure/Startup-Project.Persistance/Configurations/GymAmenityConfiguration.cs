using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Startup_Project.Domain.Entities;

namespace Startup_Project.Persistance.Configurations;

public class GymAmenityConfiguration : IEntityTypeConfiguration<GymAmenity>
{
    public void Configure(EntityTypeBuilder<GymAmenity> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.AmenityType).IsRequired();
        builder.Property(x => x.CustomName).HasMaxLength(100);
        builder.Property(x => x.IconName).HasMaxLength(50);

        builder.HasIndex(x => new { x.GymId, x.AmenityType }).IsUnique();
        builder.HasIndex(x => new { x.GymId, x.IsHighlighted });

        builder.HasOne(x => x.Gym)
               .WithMany(g => g.Amenities)
               .HasForeignKey(x => x.GymId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
