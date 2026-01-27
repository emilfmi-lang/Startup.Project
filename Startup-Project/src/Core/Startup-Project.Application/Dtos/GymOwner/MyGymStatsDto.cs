namespace Startup_Project.Application.Dtos.GymOwner;

public class MyGymStatsDto
{
    public int TotalMembers { get; set; }
    public int ActiveSubscriptions { get; set; }
    public int TodayCheckIns { get; set; }
    public int TotalPackages { get; set; }
    public int TotalReviews { get; set; }
    public double AverageRating { get; set; }
    public decimal MonthlyRevenue { get; set; }
}
