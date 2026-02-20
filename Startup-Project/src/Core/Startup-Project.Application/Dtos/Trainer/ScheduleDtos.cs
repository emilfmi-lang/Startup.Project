namespace Startup_Project.Application.Dtos.Trainer;

public class ScheduleSessionItemDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = null!;
    public string Type { get; set; } = null!;
    public string? Description { get; set; }
    public string StartTime { get; set; } = null!;
    public string EndTime { get; set; } = null!;
    public int Duration { get; set; }
    public string? MemberName { get; set; }
    public string Status { get; set; } = null!;
}

public class ScheduleDayDto
{
    public DateTime Date { get; set; }
    public string DayName { get; set; } = null!;
    public int DayNumber { get; set; }
    public bool IsToday { get; set; }
    public List<ScheduleSessionItemDto> Sessions { get; set; } = [];
}

public class WeeklyScheduleDto
{
    public DateTime WeekStart { get; set; }
    public DateTime WeekEnd { get; set; }
    public string WeekLabel { get; set; } = null!;
    public List<ScheduleDayDto> Days { get; set; } = [];
}

public class TodayScheduleDto
{
    public DateTime Date { get; set; }
    public int TotalSessions { get; set; }
    public List<ScheduleSessionItemDto> Sessions { get; set; } = [];
}
