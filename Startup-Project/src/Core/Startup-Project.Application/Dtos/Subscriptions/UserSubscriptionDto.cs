namespace Startup_Project.Application.Dtos.Subscriptions;

/// <summary>
/// User Dashboard - Active Subscriptions Tab üçün
/// </summary>
public class UserSubscriptionDto
{
    public Guid Id { get; set; }

    // Gym Info
    public Guid GymId { get; set; }
    public string GymName { get; set; } = null!;
    public string? GymImage { get; set; }

    // Package Info
    public Guid PackageId { get; set; }
    public string PackageName { get; set; } = null!;
    public decimal Price { get; set; }
    public string PriceLabel { get; set; } = null!;

    // Dates
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string DateRange { get; set; } = null!;

    // Status
    public string Status { get; set; } = null!;
    public bool IsActive { get; set; }
    public bool IsExpiringSoon { get; set; }

    // Time Remaining
    public int DaysRemaining { get; set; }
    public string TimeRemainingLabel { get; set; } = null!;
    public double ProgressPercent { get; set; }

    // Package Features
    public List<string> Features { get; set; } = [];
}
