using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Startup_Project.Domain.Entities;

namespace Startup_Project.Persistance.Configurations;

public class UserPaymentMethodConfiguration : IEntityTypeConfiguration<UserPaymentMethod>
{
    public void Configure(EntityTypeBuilder<UserPaymentMethod> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasOne(pm => pm.User)
            .WithMany(u => u.PaymentMethods)
            .HasForeignKey(pm => pm.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(pm => pm.CardHolderName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(pm => pm.Last4Digits)
            .IsRequired()
            .HasMaxLength(4)
            .IsFixedLength();

        builder.HasIndex(pm => pm.UserId)
            .HasDatabaseName("IX_UserPaymentMethods_UserId");

        builder.HasQueryFilter(pm => !pm.IsDeleted);
    }
}
