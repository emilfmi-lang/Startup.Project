namespace Startup_Project.Application.Dtos.UserSession;

/// <summary>
/// User panelind? sessiya kart? üçün DTO.
/// Screenshot: Trainer ad?, specialization, avatar, rating, tarix, saat, müdd?t, gym, status, cancel.
/// </summary>
public class UserSessionDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = null!;
    public string ExerciseType { get; set; } = null!;
    public string? Description { get; set; }

    // Trainer
    public Guid TrainerId { get; set; }
    public string TrainerName { get; set; } = null!;
    public string Specialization { get; set; } = null!;
    public string? TrainerProfileImage { get; set; }
    public double TrainerRating { get; set; }

    // Schedule
    public DateTime Date { get; set; }
    public string StartTime { get; set; } = null!;
    public string EndTime { get; set; } = null!;
    public int DurationMinutes { get; set; }

    // Location
    public Guid GymId { get; set; }
    public string GymName { get; set; } = null!;
    public bool IsOnline { get; set; }

    // Price
    public decimal Price { get; set; }
    public int MaxParticipants { get; set; }
    public int CurrentParticipants { get; set; }
    public bool IsFull { get; set; }

    // Status
    public string Status { get; set; } = null!;
    public bool CanCancel { get; set; }
    public bool CanBook { get; set; }
    public bool IsBooked { get; set; }
}
