namespace Startup_Project.Application.Dtos.Trainer;

public class AddStudentDto
{
    public Guid StudentId { get; set; }
    public string Program { get; set; } = null!;
    public int TotalSessions { get; set; }
}
