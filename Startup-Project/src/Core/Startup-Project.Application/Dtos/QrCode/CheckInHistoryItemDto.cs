namespace Startup_Project.Application.Dtos.QrCode;

public class CheckInHistoryItemDto
{
    public Guid Id { get; set; }
    public string UserFullName { get; set; } = null!;
    public string? UserProfileImage { get; set; }
    public string GymName { get; set; } = null!;
    public string? PackageName { get; set; }
    public DateTime CheckInTime { get; set; }
    public DateTime? CheckOutTime { get; set; }
    public string Status { get; set; } = null!;
    public int? DurationMinutes { get; set; }
}

public class GymCheckInStatsDto
{
    public int TodayCount { get; set; }
    public int WeekCount { get; set; }
    public int MonthCount { get; set; }
    public int CurrentlyInGym { get; set; }
}
