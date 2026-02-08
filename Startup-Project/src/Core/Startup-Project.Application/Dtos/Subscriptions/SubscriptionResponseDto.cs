namespace Startup_Project.Application.Dtos.Subscriptions;

/// <summary>
/// Sadə subscription response (admin/list üçün)
/// </summary>
public class SubscriptionResponseDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; } = null!;
    public Guid PackageId { get; set; }
    public string PackageName { get; set; } = null!;
    public string GymName { get; set; } = null!;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; }
    public decimal Price { get; set; }
}
