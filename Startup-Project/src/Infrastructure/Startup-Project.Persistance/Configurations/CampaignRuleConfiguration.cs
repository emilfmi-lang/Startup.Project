using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Startup_Project.Domain.Entities;

namespace Startup_Project.Persistance.Configurations;

public class CampaignRuleConfiguration : IEntityTypeConfiguration<CampaignRule>
{
    public void Configure(EntityTypeBuilder<CampaignRule> builder)
    {
        builder.ToTable("CampaignRules");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.RuleType)
            .IsRequired();

        builder.Property(x => x.Operator)
            .IsRequired();

        builder.Property(x => x.Value)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Description)
            .HasMaxLength(500);

        builder.Property(x => x.CampaignId)
            .IsRequired();

        builder.HasOne(x => x.Campaign)
            .WithMany(x => x.Rules)
            .HasForeignKey(x => x.CampaignId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.CampaignId);
    }
}
