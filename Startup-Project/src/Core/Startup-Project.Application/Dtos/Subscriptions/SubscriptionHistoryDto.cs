namespace Startup_Project.Application.Dtos.Subscriptions;

/// <summary>
/// User Dashboard - History Tab üçün (Payment History)
/// </summary>
public class SubscriptionHistoryDto
{
    public Guid Id { get; set; }

    // Gym Info
    public string GymName { get; set; } = null!;
    public string? GymImage { get; set; }

    // Package Info
    public string PackageName { get; set; } = null!;

    // Payment Info
    public decimal Amount { get; set; }
    public string AmountLabel { get; set; } = null!;
    public DateTime PaymentDate { get; set; }
    public string PaymentDateLabel { get; set; } = null!;
    public string PaymentStatus { get; set; } = null!;
    public bool IsPaid { get; set; }
}
