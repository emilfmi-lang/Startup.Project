using Startup_Project.Domain.Entities.Common;

namespace Startup_Project.Domain.Entities;

public class AiMessage : BaseEntity
{
    public Guid ConversationId { get; private set; }
    public AiConversation Conversation { get; private set; } = null!;

    public string Role { get; private set; } = null!;
    public string Content { get; private set; } = null!;
    public int? TokensUsed { get; private set; }

    private AiMessage() { }

    public AiMessage(Guid conversationId, string role, string content, int? tokensUsed = null)
    {
        ConversationId = conversationId;
        Role = role;
        Content = content;
        TokensUsed = tokensUsed;
        CreatedAt = DateTime.UtcNow;
    }
}
