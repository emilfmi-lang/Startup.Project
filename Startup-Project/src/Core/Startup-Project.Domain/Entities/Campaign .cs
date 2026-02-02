using Startup_Project.Domain.Entities.Common;
using Startup_Project.Domain.Enums;

namespace Startup_Project.Domain.Entities;

public class Campaign : BaseEntity
{
    public string Name { get; private set; } = null!;
    public string Description { get; private set; } = null!;
    public DiscountType DiscountType { get; private set; }
    public decimal DiscountValue { get; private set; }
    public decimal? MaxDiscountAmount { get; private set; }
    public decimal? MinimumOrderAmount { get; private set; }
    public int Priority { get; private set; }
    public bool IsStackable { get; private set; }
    public TargetType TargetType { get; private set; }
    public bool IsActive { get; private set; }
    public int? UsageLimit { get; private set; }
    public int UsedCount { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public List<CampaignRule> Rules { get; private set; } = [];
    public List<CampaignUsage> Usages { get; private set; } = [];
    public Campaign() { }
    public Campaign(string name, string description, DiscountType discountType, decimal discountValue,
                       DateTime startDate, DateTime endDate, int priority,
                       bool isStackable, TargetType targetType,
                       decimal? maxDiscountAmount = null,
                       decimal? minimumOrderAmount = null,
                       int? usageLimit = null)
    {
        Name = name;
        Description = description;
        DiscountType = discountType;
        DiscountValue = discountValue;
        StartDate = startDate;
        EndDate = endDate;
        Priority = priority;
        IsStackable = isStackable;
        TargetType = targetType;
        MaxDiscountAmount = maxDiscountAmount;
        MinimumOrderAmount = minimumOrderAmount;
        UsageLimit = usageLimit;
        CreatedAt = DateTime.UtcNow;
        if (discountValue <= 0) throw new ArgumentException("Discount value must be greater than zero.");
        if (startDate >= endDate) throw new ArgumentException("Start date must be earlier than end date.");
    }
    public bool IsValidForDate()=> DateTime.UtcNow >= StartDate 
                                   && DateTime.UtcNow <= EndDate;
}
