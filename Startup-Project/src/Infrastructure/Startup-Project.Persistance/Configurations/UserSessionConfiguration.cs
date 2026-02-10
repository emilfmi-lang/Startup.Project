using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Startup_Project.Domain.Entities;

namespace Startup_Project.Persistance.Configurations;

public class UserSessionConfiguration : IEntityTypeConfiguration<UserSession>
{
    public void Configure(EntityTypeBuilder<UserSession> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasOne(s => s.User)
            .WithMany(u => u.Sessions)
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(s => s.DeviceInfo)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(s => s.Location)
            .HasMaxLength(200);

        builder.Property(s => s.IpAddress)
            .HasMaxLength(50);

        builder.Property(s => s.UserAgent)
            .HasMaxLength(500);

        builder.Property(s => s.RefreshToken)
            .IsRequired()
            .HasMaxLength(500);

        builder.HasIndex(s => s.UserId)
            .HasDatabaseName("IX_UserSessions_UserId");

        builder.HasIndex(s => s.RefreshToken)
            .IsUnique()
            .HasDatabaseName("IX_UserSessions_RefreshToken");

        builder.HasQueryFilter(s => !s.IsDeleted);
    }
}
