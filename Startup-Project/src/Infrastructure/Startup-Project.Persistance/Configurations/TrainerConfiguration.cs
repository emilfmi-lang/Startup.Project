using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Startup_Project.Domain.Entities;

namespace Startup_Project.Persistance.Configurations;

public class TrainerConfiguration : IEntityTypeConfiguration<Trainer>
{
    public void Configure(EntityTypeBuilder<Trainer> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Specialization)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(t => t.Bio)
            .HasMaxLength(1000);

        builder.Property(t => t.CertificateUrl)
            .HasMaxLength(500);

        builder.Property(t => t.ProfileImageUrl)
            .HasMaxLength(500);

        // Hybrid Trainer Properties
        builder.Property(t => t.IsOnlineTrainer)
            .HasDefaultValue(false);

        builder.Property(t => t.IsVerified)
            .HasDefaultValue(false);

        builder.Property(t => t.VideoIntroUrl)
            .HasMaxLength(500);

        builder.Property(t => t.SocialMediaLinks)
            .HasMaxLength(1000);

        builder.Property(t => t.AverageRating)
            .HasColumnType("float")
            .HasDefaultValue(0);

        builder.Property(t => t.ReviewCount)
            .HasDefaultValue(0);

        builder.Property(t => t.TotalClients)
            .HasDefaultValue(0);

        builder.Property(t => t.SessionPrice)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(t => t.TotalSessions)
            .HasDefaultValue(0);

        builder.Property(t => t.Status)
            .IsRequired();

        // GymId nullable - Online trainer ucun
        builder.Property(t => t.GymId)
            .IsRequired(false);

        // Relations
        builder.HasOne(t => t.User)
            .WithOne(u => u.TrainerProfile)
            .HasForeignKey<Trainer>(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(t => t.Gym)
            .WithMany(g => g.Trainers)
            .HasForeignKey(t => t.GymId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        builder.HasMany(t => t.Reviews)
            .WithOne(r => r.Trainer)
            .HasForeignKey(r => r.TrainerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(t => t.Schedules)
            .WithOne(s => s.Trainer)
            .HasForeignKey(s => s.TrainerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(t => t.Sessions)
            .WithOne(s => s.Trainer)
            .HasForeignKey(s => s.TrainerId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(t => t.UserId).IsUnique();
        builder.HasIndex(t => t.GymId);
        builder.HasIndex(t => t.Status);
        builder.HasIndex(t => t.AverageRating);
        builder.HasIndex(t => t.IsOnlineTrainer);
        builder.HasIndex(t => t.IsVerified);
        builder.HasIndex(t => new { t.IsOnlineTrainer, t.Status });
    }
}
