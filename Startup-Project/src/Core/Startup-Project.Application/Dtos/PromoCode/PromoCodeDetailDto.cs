namespace Startup_Project.Application.Dtos.PromoCode;

public class PromoCodeDetailDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string DiscountType { get; set; } = null!;
    public decimal DiscountValue { get; set; }
    public decimal? MinimumOrderAmount { get; set; }
    public int UsageLimit { get; set; }
    public int UsedCount { get; set; }
    public int RemainingUsage => UsageLimit - UsedCount;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; }
    public bool IsExpired => DateTime.UtcNow > EndDate;
    public bool IsValid => IsActive && !IsExpired && RemainingUsage > 0;
    public Guid? PackageId { get; set; }
    public string? PackageName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<PromoCodeUsageDto> RecentUsages { get; set; } = [];
}
