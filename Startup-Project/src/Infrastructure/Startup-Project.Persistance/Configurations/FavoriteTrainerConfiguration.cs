using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Startup_Project.Domain.Entities;

namespace Startup_Project.Persistance.Configurations;

public class FavoriteTrainerConfiguration : IEntityTypeConfiguration<FavoriteTrainer>
{
    public void Configure(EntityTypeBuilder<FavoriteTrainer> builder)
    {
        builder.HasKey(x => x.Id);

        // User -> FavoriteTrainers (Cascade - user silinəndə favorites də silinsin)
        builder.HasOne(f => f.User)
           .WithMany(u => u.FavoriteTrainers)
           .HasForeignKey(f => f.UserId)
           .OnDelete(DeleteBehavior.Cascade);

        // Trainer -> FavoritedByUsers (NoAction - SQL Server cascade cycle problemi)
        builder.HasOne(f => f.Trainer)
           .WithMany(t => t.FavoritedByUsers)
           .HasForeignKey(f => f.TrainerId)
           .OnDelete(DeleteBehavior.NoAction);

        // Unique constraint
        builder.HasIndex(f => new { f.UserId, f.TrainerId })
            .IsUnique()
            .HasDatabaseName("IX_FavoriteTrainers_UserId_TrainerId");

        // Performance indexes
        builder.HasIndex(f => f.UserId);
        builder.HasIndex(f => f.TrainerId);
    }
}
