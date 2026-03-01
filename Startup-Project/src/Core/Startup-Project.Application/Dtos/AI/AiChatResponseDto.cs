namespace Startup_Project.Application.Dtos.AI;

public class AiChatResponseDto
{
    public Guid ConversationId { get; set; }
    public Guid MessageId { get; set; }
    public string Reply { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}
