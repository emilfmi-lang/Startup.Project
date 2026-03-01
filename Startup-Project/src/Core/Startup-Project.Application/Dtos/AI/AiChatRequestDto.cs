namespace Startup_Project.Application.Dtos.AI;

public class AiChatRequestDto
{
    public Guid? ConversationId { get; set; }
    public string Message { get; set; } = null!;
}
