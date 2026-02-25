using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Startup_Project.Domain.Entities;

namespace Startup_Project.Persistance.Configurations;

public class ConversationParticipantConfiguration : IEntityTypeConfiguration<ConversationParticipant>
{
    public void Configure(EntityTypeBuilder<ConversationParticipant> builder)
    {
        builder.HasKey(cp => cp.Id);

        builder.HasIndex(cp => new { cp.ConversationId, cp.UserId })
            .IsUnique()
            .HasFilter("IsDeleted = 0");

        builder.HasOne(cp => cp.Conversation)
            .WithMany(c => c.Participants)
            .HasForeignKey(cp => cp.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(cp => cp.User)
            .WithMany(u => u.ChatParticipations)
            .HasForeignKey(cp => cp.UserId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(cp => cp.UserId);
    }
}
