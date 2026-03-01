using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Startup_Project.Domain.Entities;

namespace Startup_Project.Persistance.Configurations;

public class AiMessageConfiguration : IEntityTypeConfiguration<AiMessage>
{
    public void Configure(EntityTypeBuilder<AiMessage> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Role)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.Content)
            .IsRequired();

        builder.HasOne(x => x.Conversation)
            .WithMany(x => x.Messages)
            .HasForeignKey(x => x.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(x => !x.IsDeleted);

        builder.HasIndex(x => new { x.ConversationId, x.CreatedAt });
    }
}
