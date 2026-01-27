using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Startup_Project.Domain.Entities.Identity;

namespace Startup_Project.Persistance.Configurations;

public class AppUserConfiguration : IEntityTypeConfiguration<AppUser>
{
    public void Configure(EntityTypeBuilder<AppUser> builder)
    {
        builder.Property(u => u.FullName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(u => u.GoogleId)
            .HasMaxLength(100);

        builder.Property(u => u.AuthProvider)
            .IsRequired()
            .HasDefaultValue(Domain.Enums.AuthProvider.Local);

        builder.HasIndex(u => u.GoogleId)
            .IsUnique()
            .HasFilter("[GoogleId] IS NOT NULL");

        builder.HasIndex(u => u.Email);
    }
}
