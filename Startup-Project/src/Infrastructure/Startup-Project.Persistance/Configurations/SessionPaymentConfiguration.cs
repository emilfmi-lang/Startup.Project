using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Startup_Project.Domain.Entities;

namespace Startup_Project.Persistance.Configurations;

public class SessionPaymentConfiguration : IEntityTypeConfiguration<SessionPayment>
{
    public void Configure(EntityTypeBuilder<SessionPayment> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Amount)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(p => p.OrderId)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(p => p.BankSessionId)
            .HasMaxLength(500);

        builder.Property(p => p.Status)
            .IsRequired();

        builder.Property(p => p.FailureReason)
            .HasMaxLength(1000);

        builder.HasOne(p => p.Student)
            .WithMany()
            .HasForeignKey(p => p.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Trainer)
            .WithMany()
            .HasForeignKey(p => p.TrainerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.ScheduledSession)
            .WithMany()
            .HasForeignKey(p => p.ScheduledSessionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(p => p.OrderId).IsUnique();
        builder.HasIndex(p => p.BankSessionId);
        builder.HasIndex(p => p.StudentId);
        builder.HasIndex(p => p.TrainerId);
        builder.HasIndex(p => p.Status);
        builder.HasIndex(p => new { p.StudentId, p.ScheduledSessionId });
    }
}
