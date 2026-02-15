namespace Startup_Project.Application.Dtos.Admin;

public class CreateSessionDto
{
    public Guid TrainerId { get; set; }
    public Guid MemberId { get; set; }
    public string ExerciseType { get; set; } = null!;
    public DateTime ScheduledTime { get; set; }
    public int DurationMinutes { get; set; }
}
