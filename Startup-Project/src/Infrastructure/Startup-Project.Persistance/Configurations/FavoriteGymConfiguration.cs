using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Startup_Project.Domain.Entities;

namespace Startup_Project.Persistance.Configurations;

public class FavoriteGymConfiguration : IEntityTypeConfiguration<FavoriteGym>
{
    public void Configure(EntityTypeBuilder<FavoriteGym> builder)
    {
        builder.HasKey(f => f.Id);

        builder.HasOne(f => f.User)
            .WithMany(u => u.FavoriteGyms)
            .HasForeignKey(f => f.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(f => f.Gym)
            .WithMany(g => g.FavoritedByUsers)
            .HasForeignKey(f => f.GymId)
            .OnDelete(DeleteBehavior.Cascade);

        // Unique constraint: User can favorite a gym only once
        builder.HasIndex(f => new { f.UserId, f.GymId })
            .IsUnique()
            .HasDatabaseName("IX_FavoriteGyms_UserId_GymId");

        // Performance indexes
        builder.HasIndex(f => f.UserId);
        builder.HasIndex(f => f.GymId);
    }
}
