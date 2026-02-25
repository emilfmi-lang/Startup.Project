using Startup_Project.Domain.Entities.Common;
using Startup_Project.Domain.Entities.Identity;

namespace Startup_Project.Domain.Entities;

public class ConversationParticipant : BaseEntity
{
    public Guid ConversationId { get; private set; }
    public Conversation Conversation { get; private set; } = null!;

    public Guid UserId { get; private set; }
    public AppUser User { get; private set; } = null!;

    public int UnreadCount { get; private set; } = 0;
    public bool IsArchived { get; private set; } = false;
    public DateTime? LastReadAt { get; private set; }

    private ConversationParticipant() { }

    public ConversationParticipant(Guid conversationId, Guid userId)
    {
        ConversationId = conversationId;
        UserId = userId;
    }

    public void IncrementUnread()
    {
        UnreadCount++;
    }

    public void MarkAsRead()
    {
        UnreadCount = 0;
        LastReadAt = DateTime.UtcNow;
    }

    public void Archive()
    {
        IsArchived = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Unarchive()
    {
        IsArchived = false;
        UpdatedAt = DateTime.UtcNow;
    }
}
