using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Startup_Project.Domain.Entities;

namespace Startup_Project.Persistance.Configurations;

public class TrainerReviewConfiguration : IEntityTypeConfiguration<TrainerReview>
{
    public void Configure(EntityTypeBuilder<TrainerReview> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.StarCount)
            .IsRequired();

        builder.Property(r => r.Comment)
            .HasMaxLength(500);

        builder.HasOne(r => r.Trainer)
            .WithMany(t => t.Reviews)
            .HasForeignKey(r => r.TrainerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.User)
            .WithMany(u => u.TrainerReviews)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(r => new { r.TrainerId, r.UserId })
            .IsUnique()
            .HasFilter("IsDeleted = 0");
        builder.HasIndex(r => r.TrainerId);
    }
}
