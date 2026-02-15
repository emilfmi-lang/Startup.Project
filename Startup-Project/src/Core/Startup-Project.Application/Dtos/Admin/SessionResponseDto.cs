namespace Startup_Project.Application.Dtos.Admin;

public class SessionResponseDto
{
    public Guid Id { get; set; }
    public Guid TrainerId { get; set; }
    public string TrainerName { get; set; } = null!;
    public Guid? MemberId { get; set; }
    public string? MemberName { get; set; }
    public string ExerciseType { get; set; } = null!;
    public DateTime ScheduledTime { get; set; }
    public int DurationMinutes { get; set; }
    public string Status { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}
