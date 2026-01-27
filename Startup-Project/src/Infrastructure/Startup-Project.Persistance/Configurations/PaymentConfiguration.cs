using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Startup_Project.Domain.Entities;

namespace Startup_Project.Persistance.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Amount)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(p => p.SessionId)
            .HasMaxLength(500);

        builder.Property(p => p.Status)
            .IsRequired();

        builder.HasOne(p => p.User)
            .WithMany()
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(p => p.Package)
            .WithMany()
            .HasForeignKey(p => p.PackageId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(p => p.SessionId);
        builder.HasIndex(p => p.UserId);
        builder.HasIndex(p => p.Status);
    }
}
