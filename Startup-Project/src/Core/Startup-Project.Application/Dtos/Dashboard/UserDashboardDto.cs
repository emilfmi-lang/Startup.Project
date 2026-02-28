namespace Startup_Project.Application.Dtos.Dashboard;

public class UserDashboardDto
{
    public ActiveSubscriptionDto? ActiveSubscription { get; set; }
    public ThisMonthStatsDto ThisMonth { get; set; } = null!;
    public List<DashboardActivityDto> RecentActivities { get; set; } = [];
}

public class ActiveSubscriptionDto
{
    public Guid SubscriptionId { get; set; }
    public string PackageName { get; set; } = null!;
    public string GymName { get; set; } = null!;
    public string? GymImage { get; set; }
    public int DaysRemaining { get; set; }
    public int ElapsedDays { get; set; }
    public int TotalDays { get; set; }
    public double ProgressPercent { get; set; }
    public string ProgressLabel { get; set; } = null!;
    public string DaysRemainingLabel { get; set; } = null!;
    public string Status { get; set; } = null!;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

public class ThisMonthStatsDto
{
    public int GymVisits { get; set; }
    public string TotalTime { get; set; } = null!;
    public double TotalTimeHours { get; set; }
    public int Sessions { get; set; }
}

public class DashboardActivityDto
{
    public Guid Id { get; set; }
    public string Type { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string? Subtitle { get; set; }
    public string Icon { get; set; } = null!;
    public DateTime Date { get; set; }
    public string DateLabel { get; set; } = null!;
}
