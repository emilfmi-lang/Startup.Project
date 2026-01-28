using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Startup_Project.Domain.Entities;

namespace Startup_Project.Persistance.Configurations;

public class TrainingSessionConfiguration : IEntityTypeConfiguration<TrainingSession>
{
    public void Configure(EntityTypeBuilder<TrainingSession> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Price)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(s => s.QrToken)
            .HasMaxLength(100);

        builder.Property(s => s.Status)
            .IsRequired();

        builder.Property(s => s.CancellationReason)
            .HasMaxLength(500);

        builder.HasOne(s => s.Trainer)
            .WithMany(t => t.Sessions)
            .HasForeignKey(s => s.TrainerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.User)
            .WithMany(u => u.TrainingSessions)
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(s => s.Gym)
            .WithMany(g => g.TrainingSessions)
            .HasForeignKey(s => s.GymId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.Payment)
            .WithMany()
            .HasForeignKey(s => s.PaymentId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(s => s.TrainerId);
        builder.HasIndex(s => s.UserId);
        builder.HasIndex(s => s.GymId);
        builder.HasIndex(s => s.Status);
        builder.HasIndex(s => s.QrToken).IsUnique().HasFilter("[QrToken] IS NOT NULL");
        builder.HasIndex(s => s.ExpiresAt);
    }
}
