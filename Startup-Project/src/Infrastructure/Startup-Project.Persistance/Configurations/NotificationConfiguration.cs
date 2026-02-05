using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Startup_Project.Domain.Entities;

namespace Startup_Project.Persistance.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Body).IsRequired().HasMaxLength(1000);

        builder.HasOne(x => x.User)
               .WithMany() 
               .HasForeignKey(x => x.UserId)
               .OnDelete(DeleteBehavior.Cascade);
        builder.Property(x => x.Type).IsRequired();
        builder.Property(x => x.Channel).IsRequired();
        builder.Property(x => x.Status).IsRequired();
        builder.Property(x => x.ImageUrl).HasMaxLength(500);
        builder.Property(x => x.ActionUrl).HasMaxLength(500);

        builder.HasIndex(x => new { x.UserId, x.Status });
        builder.HasIndex(x => x.CreatedAt);
    }
}
