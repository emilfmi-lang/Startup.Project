namespace Startup_Project.Application.Dtos.AI;

public class AiConversationListDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = null!;
    public string LastMessage { get; set; } = null!;
    public int MessageCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
