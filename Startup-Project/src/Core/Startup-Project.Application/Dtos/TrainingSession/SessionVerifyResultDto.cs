namespace Startup_Project.Application.Dtos.TrainingSession;

public class SessionVerifyResultDto
{
    public bool IsValid { get; set; }
    public string Message { get; set; } = null!;
    public Guid? SessionId { get; set; }
    public string? UserName { get; set; }
    public string? TrainerName { get; set; }
    public string? GymName { get; set; }
    public DateTime? UsedAt { get; set; }
}
