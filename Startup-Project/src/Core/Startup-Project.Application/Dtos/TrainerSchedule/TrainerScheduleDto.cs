namespace Startup_Project.Application.Dtos.TrainerSchedule;

public class TrainerScheduleDto
{
    public DayOfWeek DayOfWeek { get; set; }
    public string StartTime { get; set; } = null!;  
    public string EndTime { get; set; } = null!;    
    public bool IsAvailable { get; set; }
}
