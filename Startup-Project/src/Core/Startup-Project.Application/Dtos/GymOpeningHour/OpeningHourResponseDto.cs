namespace Startup_Project.Application.Dtos.GymOpeningHour;

public class OpeningHourResponseDto
{
    public string Day { get; set; } = null!;       
    public string OpenTime { get; set; } = null!;  
    public string CloseTime { get; set; } = null!; 
    public bool IsClosed { get; set; }
    public string DisplayText { get; set; } = null!;
}
