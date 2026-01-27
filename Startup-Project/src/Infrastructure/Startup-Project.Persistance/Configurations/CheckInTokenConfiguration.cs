using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Startup_Project.Domain.Entities;

namespace Startup_Project.Persistance.Configurations;

public class CheckInTokenConfiguration : IEntityTypeConfiguration<CheckInToken>
{
    public void Configure(EntityTypeBuilder<CheckInToken> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Token)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(c => c.ExpiryDate)
            .IsRequired();

        builder.Property(c => c.IsUsed)
            .HasDefaultValue(false);

        builder.HasOne(c => c.User)
            .WithMany()
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(c => c.Token).IsUnique();
        builder.HasIndex(c => c.UserId);
        builder.HasIndex(c => c.ExpiryDate);
    }
}
