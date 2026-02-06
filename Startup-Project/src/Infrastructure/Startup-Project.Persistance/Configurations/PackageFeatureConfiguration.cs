using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Startup_Project.Domain.Entities;

namespace Startup_Project.Persistance.Configurations;

public class PackageFeatureConfiguration : IEntityTypeConfiguration<PackageFeature>
{
    public void Configure(EntityTypeBuilder<PackageFeature> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
               .IsRequired()
               .HasMaxLength(200);

        builder.HasIndex(x => new { x.PackageId, x.DisplayOrder });

        builder.HasOne(x => x.Package)
               .WithMany(p => p.Features)
               .HasForeignKey(x => x.PackageId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
