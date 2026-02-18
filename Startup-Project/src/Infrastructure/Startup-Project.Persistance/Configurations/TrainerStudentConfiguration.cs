using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Startup_Project.Domain.Entities;

namespace Startup_Project.Persistance.Configurations;

public class TrainerStudentConfiguration : IEntityTypeConfiguration<TrainerStudent>
{
    public void Configure(EntityTypeBuilder<TrainerStudent> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Program)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.TotalSessions)
            .IsRequired();

        builder.HasOne(x => x.Trainer)
            .WithMany(t => t.Students)
            .HasForeignKey(x => x.TrainerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Student)
            .WithMany()
            .HasForeignKey(x => x.StudentId)
            .OnDelete(DeleteBehavior.Cascade);

        // Eyni trainer-e eyni student iki defe elave olunmasin
        builder.HasIndex(x => new { x.TrainerId, x.StudentId })
            .IsUnique()
            .HasDatabaseName("IX_TrainerStudents_TrainerId_StudentId");

        builder.HasIndex(x => x.TrainerId);
        builder.HasIndex(x => x.StudentId);
    }
}
