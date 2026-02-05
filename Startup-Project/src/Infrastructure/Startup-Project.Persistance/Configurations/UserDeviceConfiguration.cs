using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Startup_Project.Domain.Entities;

namespace Startup_Project.Persistance.Configurations;

public class UserDeviceConfiguration : IEntityTypeConfiguration<UserDevice>
{
    public void Configure(EntityTypeBuilder<UserDevice> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.DeviceToken)
               .IsRequired()
               .HasMaxLength(500);

        builder.Property(x => x.Platform)
               .IsRequired();

        builder.Property(x => x.DeviceModel)
               .HasMaxLength(100);

        builder.Property(x => x.AppVersion)
               .HasMaxLength(50);

        builder.Property(x => x.LastActiveAt)
               .IsRequired();

        builder.HasIndex(x => x.DeviceToken).IsUnique();
        builder.HasIndex(x => new { x.UserId, x.IsActive });

        builder.HasOne(x => x.User)
               .WithMany(u => u.Devices)
               .HasForeignKey(x => x.UserId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
