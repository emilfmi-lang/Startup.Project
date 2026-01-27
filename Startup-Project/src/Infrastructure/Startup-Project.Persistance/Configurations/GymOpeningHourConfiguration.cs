using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Startup_Project.Domain.Entities;

namespace Startup_Project.Persistance.Configurations;

public class GymOpeningHourConfiguration : IEntityTypeConfiguration<GymOpeningHour>
{
    public void Configure(EntityTypeBuilder<GymOpeningHour> builder)
    {
        builder.HasKey(g => g.Id);

        builder.Property(g => g.DayOfWeek)
            .IsRequired();

        builder.Property(g => g.OpenTime)
            .IsRequired();

        builder.Property(g => g.ClosingTime)
            .IsRequired();

        builder.Property(g => g.IsClosed)
            .HasDefaultValue(false);

        builder.HasOne(g => g.Gym)
            .WithMany(gym => gym.OpeningHours)
            .HasForeignKey(g => g.GymId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(g => new { g.GymId, g.DayOfWeek }).IsUnique();
    }
}
