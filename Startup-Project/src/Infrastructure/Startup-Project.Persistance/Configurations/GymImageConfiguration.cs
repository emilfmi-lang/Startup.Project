using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Startup_Project.Domain.Entities;

namespace Startup_Project.Persistance.Configurations;

public class GymImageConfiguration : IEntityTypeConfiguration<GymImage>
{
    public void Configure(EntityTypeBuilder<GymImage> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ImagePath)
               .IsRequired()
               .HasMaxLength(500);

        builder.Property(x => x.Alt)
               .HasMaxLength(200);

        builder.HasIndex(x => new { x.GymId, x.DisplayOrder });
        builder.HasIndex(x => new { x.GymId, x.IsMain });

        builder.HasOne(x => x.Gym)
               .WithMany(g => g.Images)
               .HasForeignKey(x => x.GymId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
