

namespace Startup_Project.Application.Dtos.GymOpeningHour;

public class OpeningHourDto
{
    public DayOfWeek DayOfWeek { get; set; }
    public string OpenTime { get; set; } 
    public string ClosingTime { get; set; }
    public bool IsClosed { get; set; }
}
