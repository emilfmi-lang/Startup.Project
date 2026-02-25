using Startup_Project.Domain.Entities.Common;
using Startup_Project.Domain.Entities.Identity;

namespace Startup_Project.Domain.Entities;

public class ChatMessage : BaseEntity
{
    public Guid ConversationId { get; private set; }
    public Conversation Conversation { get; private set; } = null!;

    public Guid SenderId { get; private set; }
    public AppUser Sender { get; private set; } = null!;

    public string Content { get; private set; } = null!;

    public bool IsRead { get; private set; } = false;
    public DateTime? ReadAt { get; private set; }

    private ChatMessage() { }

    public ChatMessage(Guid conversationId, Guid senderId, string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Mesaj bo? ola bilm?z.");

        ConversationId = conversationId;
        SenderId = senderId;
        Content = content.Trim();
        CreatedAt = DateTime.UtcNow;
    }

    public void MarkAsRead()
    {
        if (!IsRead)
        {
            IsRead = true;
            ReadAt = DateTime.UtcNow;
        }
    }
}
