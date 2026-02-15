namespace Startup_Project.Application.Dtos.Trainer;

/// <summary>
/// Trainer panelinden seans yaratmaq ucun DTO
/// TrainerId JWT-den avtomatik goturulur — body-de gonderilmir
/// </summary>
public class TrainerCreateSessionDto
{
    public string Title { get; set; } = null!;
    public string Type { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime Date { get; set; }
    public string StartTime { get; set; } = null!;
    public int Duration { get; set; }
    public int MaxParticipants { get; set; }
    public decimal PricePerPerson { get; set; }
}
