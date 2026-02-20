namespace Startup_Project.Application.Dtos.Trainer;

public class CreateScheduleEntryDto
{
    public string Title { get; set; } = null!;
    public string Type { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime Date { get; set; }
    public string StartTime { get; set; } = null!;
    public int Duration { get; set; }
    public int MaxParticipants { get; set; } = 1;
    public decimal PricePerPerson { get; set; }
}

public class UpdateScheduleEntryDto
{
    public string Title { get; set; } = null!;
    public string Type { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime Date { get; set; }
    public string StartTime { get; set; } = null!;
    public int Duration { get; set; }
    public int MaxParticipants { get; set; } = 1;
    public decimal PricePerPerson { get; set; }
}

public class BulkUpdateScheduleDto
{
    public DateTime WeekStart { get; set; }
    public List<CreateScheduleEntryDto> Entries { get; set; } = [];
}
