using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Startup_Project.Domain.Entities;

namespace Startup_Project.Persistance.Configurations;

public class EmailOtpConfiguration : IEntityTypeConfiguration<EmailOtp>
{
    public void Configure(EntityTypeBuilder<EmailOtp> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(x => x.Code)
            .IsRequired()
            .HasMaxLength(6);

        builder.Property(x => x.ExpiryDate)
            .IsRequired();

        builder.Property(x => x.IsUsed)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.AttemptCount)
            .IsRequired()
            .HasDefaultValue(0);

        // Performance indexes
        builder.HasIndex(x => x.Email);
        builder.HasIndex(x => new { x.Email, x.IsUsed });
        builder.HasIndex(x => new { x.Email, x.Code });
        builder.HasIndex(x => new { x.Email, x.IsUsed, x.ExpiryDate });
    }
}
