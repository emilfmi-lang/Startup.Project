namespace Startup_Project.Application.Dtos.Admin;

public class DashboardStatsDto
{
    public int TotalUsers { get; set; }
    public int TotalGyms { get; set; }
    public int TotalActiveSubscriptions { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal MonthlyRevenue { get; set; }
    public int NewUsersThisMonth { get; set; }
    public int TodayCheckIns { get; set; }
}
