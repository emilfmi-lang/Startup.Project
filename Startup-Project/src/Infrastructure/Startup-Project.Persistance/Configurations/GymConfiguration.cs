using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Startup_Project.Domain.Entities;

namespace Startup_Project.Persistance.Configurations;

public class GymConfiguration : IEntityTypeConfiguration<Gym>
{
    public void Configure(EntityTypeBuilder<Gym> builder)
    {
        builder.HasKey(g => g.Id);

        builder.Property(g => g.Name)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(g => g.Address)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(g => g.ImagePath)
            .HasMaxLength(500);

        builder.Property(g => g.AverageRating)
            .HasColumnType("float")
            .HasDefaultValue(0);

        builder.Property(g => g.ReviewCount)
            .HasDefaultValue(0);

        builder.Property(g => g.IsDeleted)
            .HasDefaultValue(false);

        builder.HasOne(g => g.Owner)
            .WithMany(u => u.Gyms)
            .HasForeignKey(g => g.OwnerId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        builder.HasMany(g => g.Reviews)
            .WithOne(r => r.Gym)
            .HasForeignKey(r => r.GymId)
            .OnDelete(DeleteBehavior.Cascade);

                builder.HasMany(g => g.OpeningHours)
                    .WithOne(oh => oh.Gym)
                    .HasForeignKey(oh => oh.GymId)
                    .OnDelete(DeleteBehavior.Cascade);

                builder.HasMany(g => g.Amenities)
                    .WithOne(a => a.Gym)
                    .HasForeignKey(a => a.GymId)
                    .OnDelete(DeleteBehavior.Cascade);

                builder.HasMany(g => g.Packages)
                    .WithOne(p => p.Gym)
                    .HasForeignKey(p => p.GymId)
                    .OnDelete(DeleteBehavior.Cascade);

                builder.HasIndex(g => g.Name);
                builder.HasIndex(g => g.OwnerId);
                builder.HasIndex(g => g.IsDeleted);
            }
        }
