using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Startup_Project.Domain.Entities;

namespace Startup_Project.Persistance.Configurations;

public class SmsOtpConfiguration : IEntityTypeConfiguration<SmsOtp>
{
    public void Configure(EntityTypeBuilder<SmsOtp> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.PhoneNumber)
            .IsRequired()
            .HasMaxLength(20);

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
        builder.HasIndex(x => x.PhoneNumber);
        builder.HasIndex(x => new { x.PhoneNumber, x.IsUsed });
        builder.HasIndex(x => new { x.PhoneNumber, x.Code });
        builder.HasIndex(x => new { x.PhoneNumber, x.IsUsed, x.ExpiryDate });
    }
}
