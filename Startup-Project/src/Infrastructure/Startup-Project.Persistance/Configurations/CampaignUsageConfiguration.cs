using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Startup_Project.Domain.Entities;

namespace Startup_Project.Persistance.Configurations;

public class CampaignUsageConfiguration : IEntityTypeConfiguration<CampaignUsage>
{
    public void Configure(EntityTypeBuilder<CampaignUsage> builder)
    {
        builder.ToTable("CampaignUsages");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.UsedAt)
            .IsRequired();

        builder.Property(x => x.DiscountAmount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(x => x.CampaignId)
            .IsRequired();

        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.PackageId)
            .IsRequired();

        builder.HasIndex(x => new { x.CampaignId, x.UserId });

        builder.HasIndex(x => x.UsedAt);

        builder.HasOne(x => x.Campaign)
            .WithMany(x => x.Usages)
            .HasForeignKey(x => x.CampaignId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
