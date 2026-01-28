namespace Startup_Project.Application.Dtos.TrainingSession;

public class SessionDto
{
    public Guid Id { get; set; }
    public Guid TrainerId { get; set; }
    public string TrainerName { get; set; } = null!;
    public string TrainerSpecialization { get; set; } = null!;
    public string? TrainerProfileImage { get; set; }
    public Guid GymId { get; set; }
    public string GymName { get; set; } = null!;
    public string GymAddress { get; set; } = null!;
    public decimal Price { get; set; }
    public string Status { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public DateTime? UsedAt { get; set; }
    public bool IsExpired { get; set; }
    public bool CanBeUsed { get; set; }    
    public int? DaysUntilExpiry { get; set; }

}
