namespace Startup_Project.Application.Dtos.Admin;

public class GymAdminDashboardDto
{
    public Guid GymId { get; set; }
    public string GymName { get; set; } = null!;
    public string GymAddress { get; set; } = null!;
    public string? GymImagePath { get; set; }
    public bool IsOpen { get; set; }
    public double AverageRating { get; set; }
    public int ReviewCount { get; set; }

    public int TotalMembers { get; set; }
    public int ActiveMembers { get; set; }
    public int PremiumPlans { get; set; }
    public int CheckInsTotal { get; set; }
    public int TotalTrainers { get; set; }
    public int VerifiedTrainers { get; set; }
    public int TodayCheckIns { get; set; }
    public decimal MonthlyRevenue { get; set; }
}




