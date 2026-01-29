using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Startup_Project.Domain.Entities;

namespace Startup_Project.Persistance.Configurations;

public class CheckInLogConfiguration : IEntityTypeConfiguration<CheckInLog>
{
    public void Configure(EntityTypeBuilder<CheckInLog> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.CheckInTime)
            .IsRequired();

        builder.Property(c => c.CheckOutTime);

        builder.Property(c => c.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(c => c.TokenUsed)
            .HasMaxLength(100);

        builder.Property(c => c.FailureReason)
            .HasMaxLength(500);

        builder.HasOne(c => c.User)
            .WithMany()
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(c => c.Gym)
            .WithMany()
            .HasForeignKey(c => c.GymId)
            .OnDelete(DeleteBehavior.Restrict);

        // Performance indexes
        builder.HasIndex(c => c.UserId);
        builder.HasIndex(c => c.GymId);
        builder.HasIndex(c => c.Status);
        builder.HasIndex(c => c.CheckInTime);
        builder.HasIndex(c => new { c.UserId, c.CheckInTime })
            .IsDescending(false, true);
        builder.HasIndex(c => new { c.UserId, c.Status });
    }
}

