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

        builder.HasOne(c => c.User)
            .WithMany()
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(c => c.UserId);
        builder.HasIndex(c => c.CheckInTime);
        builder.HasIndex(c => new { c.UserId, c.CheckInTime });
    }
}
