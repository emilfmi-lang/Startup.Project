using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Startup_Project.Domain.Entities;

namespace Startup_Project.Persistance.Configurations;

public class ScheduledSessionConfiguration : IEntityTypeConfiguration<ScheduledSession>
{
    public void Configure(EntityTypeBuilder<ScheduledSession> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Title)
            .HasMaxLength(200)
            .IsRequired(false);

        builder.Property(s => s.ExerciseType)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(s => s.Description)
            .HasMaxLength(2000)
            .IsRequired(false);

        builder.Property(s => s.ScheduledTime)
            .IsRequired();

        builder.Property(s => s.DurationMinutes)
            .IsRequired();

        builder.Property(s => s.MaxParticipants)
            .IsRequired()
            .HasDefaultValue(1);

        builder.Property(s => s.PricePerPerson)
            .HasColumnType("decimal(18,2)")
            .HasDefaultValue(0m);

        builder.Property(s => s.Status)
            .IsRequired();

        // MemberId nullable — trainer panel seanslari ucun null olur
        builder.Property(s => s.MemberId)
            .IsRequired(false);

        // Relations
        builder.HasOne(s => s.Trainer)
            .WithMany(t => t.ScheduledSessions)
            .HasForeignKey(s => s.TrainerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.Member)
            .WithMany(u => u.ScheduledSessions)
            .HasForeignKey(s => s.MemberId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired(false);

        builder.HasOne(s => s.Gym)
            .WithMany(g => g.ScheduledSessions)
            .HasForeignKey(s => s.GymId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(s => s.TrainerId);
        builder.HasIndex(s => s.MemberId);
        builder.HasIndex(s => s.GymId);
        builder.HasIndex(s => s.Status);
        builder.HasIndex(s => s.ScheduledTime);
        builder.HasIndex(s => new { s.TrainerId, s.ScheduledTime });
    }
}
