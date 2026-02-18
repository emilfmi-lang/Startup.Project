namespace Startup_Project.Application.Dtos.Trainer;

public class TrainerSessionResponseDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = null!;
    public string Type { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime Date { get; set; }
    public string StartTime { get; set; } = null!;
    public string EndTime { get; set; } = null!;
    public int Duration { get; set; }
    public int MaxParticipants { get; set; }
    public decimal PricePerPerson { get; set; }
    public string Status { get; set; } = null!;
    public string TrainerName { get; set; } = null!;
    public string? MemberName { get; set; }
    public DateTime CreatedAt { get; set; }
}
