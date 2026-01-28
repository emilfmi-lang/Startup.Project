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

        builder.Property(t => t.AverageRating)
            .HasColumnType("float")
            .HasDefaultValue(0);

        builder.Property(t => t.ReviewCount)
            .HasDefaultValue(0);

        builder.Property(t => t.TotalClients)
            .HasDefaultValue(0);

        builder.Property(t => t.TotalSessions)
            .HasDefaultValue(0);

        builder.Property(t => t.Status)
            .IsRequired();

        builder.HasOne(t => t.User)
            .WithOne(u => u.TrainerProfile)
            .HasForeignKey<Trainer>(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(t => t.Gym)
            .WithMany(g => g.Trainers)
            .HasForeignKey(t => t.GymId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(t => t.Reviews)
            .WithOne(r => r.Trainer)
            .HasForeignKey(r => r.TrainerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(t => t.Schedules)
            .WithOne(s => s.Trainer)
            .HasForeignKey(s => s.TrainerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(t => t.UserId).IsUnique();
        builder.HasIndex(t => t.GymId);
        builder.HasIndex(t => t.Status);
        builder.HasIndex(t => t.AverageRating);
    }
}
