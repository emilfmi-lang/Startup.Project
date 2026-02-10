namespace Startup_Project.Application.Dtos.Account;

public class ActiveSessionsResponseDto
{
    public List<ActiveSessionDto> Sessions { get; set; } = [];
    public int TotalCount { get; set; }

}
