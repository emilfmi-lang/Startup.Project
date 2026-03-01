namespace Startup_Project.Application.Dtos.AI;

public class AiMessageDto
{
    public Guid Id { get; set; }
    public string Role { get; set; } = null!;
    public string Content { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}
