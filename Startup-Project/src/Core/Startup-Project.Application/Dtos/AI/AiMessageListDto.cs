namespace Startup_Project.Application.Dtos.AI;

public class AiMessageListDto
{
    public List<AiMessageDto> Messages { get; set; } = [];
    public int TotalCount { get; set; }
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}
