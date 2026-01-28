using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Startup_Project.Domain.Entities;

namespace Startup_Project.Persistance.Configurations;

public class TrainerScheduleConfiguration : IEntityTypeConfiguration<TrainerSchedule>
{
    public void Configure(EntityTypeBuilder<TrainerSchedule> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.DayOfWeek)
            .IsRequired();

        builder.Property(s => s.StartTime)
            .IsRequired();

        builder.Property(s => s.EndTime)
            .IsRequired();

        builder.Property(s => s.IsAvailable)
            .HasDefaultValue(true);

        builder.HasOne(s => s.Trainer)
            .WithMany(t => t.Schedules)
            .HasForeignKey(s => s.TrainerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(s => new { s.TrainerId, s.DayOfWeek }).IsUnique();
    }
}
