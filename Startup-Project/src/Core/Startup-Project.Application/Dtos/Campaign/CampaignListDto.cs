using Startup_Project.Domain.Enums;

namespace Startup_Project.Application.Dtos.Campaign;

public class CampaignListDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string DiscountType { get; set; } = null!;
    public decimal DiscountValue { get; set; }
    public int Priority { get; set; }
    public bool IsStackable { get; set; }
    public string TargetType { get; set; } = null!;
    public bool IsActive { get; set; }
    public int? UsageLimit { get; set; }
    public int UsedCount { get; set; }
    public int RemainingUsage => UsageLimit.HasValue ? UsageLimit.Value - UsedCount : int.MaxValue;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsExpired => DateTime.UtcNow > EndDate;
    public bool IsValid => IsActive && !IsExpired && (!UsageLimit.HasValue || UsedCount < UsageLimit.Value);
}
