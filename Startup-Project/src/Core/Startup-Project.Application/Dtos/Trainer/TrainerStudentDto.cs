namespace Startup_Project.Application.Dtos.Trainer;

public class TrainerStudentDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = null!;
    public string? ProfileImageUrl { get; set; }
    public string Program { get; set; } = null!;
    public int CompletedSessions { get; set; }
    public int TotalSessions { get; set; }
    public int ProgressPercent { get; set; }
    public string Status { get; set; } = null!;
    public DateTime? NextSessionDate { get; set; }
}
