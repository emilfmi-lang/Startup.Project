using Startup_Project.Domain.Entities.Common;

namespace Startup_Project.Domain.Entities;

public class Conversation : BaseEntity
{
    public DateTime? LastMessageAt { get; private set; }
    public string? LastMessagePreview { get; private set; }

    public List<ConversationParticipant> Participants { get; private set; } = [];
    public List<ChatMessage> Messages { get; private set; } = [];

    private Conversation() { }

    public Conversation(Guid id)
    {
        Id = id;
        CreatedAt = DateTime.UtcNow;
    }

    public void UpdateLastMessage(string? preview)
    {
        LastMessageAt = DateTime.UtcNow;
        LastMessagePreview = preview?.Length > 100
            ? preview[..100] + "..."
            : preview;
        UpdatedAt = DateTime.UtcNow;
    }
}
