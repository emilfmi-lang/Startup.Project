namespace Startup_Project.Application.Dtos.Trainer;

public class TrainerStudentsPageDto
{
    public int TotalStudents { get; set; }
    public int AvgProgress { get; set; }
    public int SessionsToday { get; set; }
    public int ActiveNow { get; set; }
    public List<TrainerStudentDto> Students { get; set; } = [];
}
