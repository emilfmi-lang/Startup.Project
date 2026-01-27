using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Startup_Project.Domain.Entities;

namespace Startup_Project.Persistance.Configurations;

public class GymReviewConfiguration : IEntityTypeConfiguration<GymReview>
{
    public void Configure(EntityTypeBuilder<GymReview> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Comment)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(r => r.StarCount)
            .IsRequired();

        builder.HasOne(r => r.Gym)
            .WithMany(g => g.Reviews)
            .HasForeignKey(r => r.GymId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.User)
            .WithMany(u => u.Reviews)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(r => r.GymId);
        builder.HasIndex(r => r.UserId);
        builder.HasIndex(r => new { r.GymId, r.UserId }).IsUnique();
    }
}
