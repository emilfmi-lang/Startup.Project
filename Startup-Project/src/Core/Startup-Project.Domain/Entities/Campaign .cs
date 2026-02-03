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
    public bool IsActive { get; private set; } = true;
    public int? UsageLimit { get; private set; }
    public int UsedCount { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public List<CampaignRule> Rules { get; private set; } = [];
    public List<CampaignUsage> Usages { get; private set; } = [];

    private Campaign() { }

    public Campaign(string name, string description, DiscountType discountType, decimal discountValue,
                    DateTime startDate, DateTime endDate, int priority,
                    bool isStackable, TargetType targetType,
                    decimal? maxDiscountAmount = null,
                    decimal? minimumOrderAmount = null,
                    int? usageLimit = null)
    {
        if (discountValue <= 0)
            throw new ArgumentException("Endirim dəyəri 0-dan böyük olmalıdır.");
        if (startDate >= endDate)
            throw new ArgumentException("Başlama tarixi bitmə tarixindən əvvəl olmalıdır.");
        if (discountType == DiscountType.Percentage && discountValue > 100)
            throw new ArgumentException("Faiz endirimi 100-dən çox ola bilməz.");

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
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }


    public bool IsValidForDate() => DateTime.UtcNow >= StartDate && DateTime.UtcNow <= EndDate;

    public bool HasAvailableUsage() => !UsageLimit.HasValue || UsedCount < UsageLimit.Value;

    public bool IsApplicableToOrder(decimal orderAmount) =>
        !MinimumOrderAmount.HasValue || orderAmount >= MinimumOrderAmount.Value;

    public bool CanBeApplied(decimal orderAmount) =>
        IsActive && IsValidForDate() && HasAvailableUsage() && IsApplicableToOrder(orderAmount);


    public decimal CalculateDiscount(decimal originalPrice)
    {
        var discount = DiscountType == DiscountType.Percentage
            ? originalPrice * (DiscountValue / 100)
            : Math.Min(DiscountValue, originalPrice);

        if (MaxDiscountAmount.HasValue && discount > MaxDiscountAmount.Value)
            discount = MaxDiscountAmount.Value;

        return Math.Round(discount, 2);
    }


    public void IncrementUsedCount()
    {
        UsedCount++;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Update(string? name, string? description, decimal? discountValue,
                       decimal? maxDiscountAmount, decimal? minimumOrderAmount,
                       int? priority, bool? isStackable, int? usageLimit,
                       DateTime? startDate, DateTime? endDate)
    {
        if (!string.IsNullOrWhiteSpace(name))
            Name = name;

        if (!string.IsNullOrWhiteSpace(description))
            Description = description;

        if (discountValue.HasValue && discountValue.Value > 0)
            DiscountValue = discountValue.Value;

        if (maxDiscountAmount.HasValue)
            MaxDiscountAmount = maxDiscountAmount.Value > 0 ? maxDiscountAmount.Value : null;

        if (minimumOrderAmount.HasValue)
            MinimumOrderAmount = minimumOrderAmount.Value > 0 ? minimumOrderAmount.Value : null;

        if (priority.HasValue && priority.Value >= 1)
            Priority = priority.Value;

        if (isStackable.HasValue)
            IsStackable = isStackable.Value;

        if (usageLimit.HasValue)
            UsageLimit = usageLimit.Value > 0 ? usageLimit.Value : null;

        if (startDate.HasValue)
            StartDate = startDate.Value;

        if (endDate.HasValue)
            EndDate = endDate.Value;

        UpdatedAt = DateTime.UtcNow;
    }

    public void AddRule(CampaignRule rule)
    {
        Rules.Add(rule);
        UpdatedAt = DateTime.UtcNow;
    }

    public void RemoveRule(Guid ruleId)
    {
        var rule = Rules.FirstOrDefault(r => r.Id == ruleId);
        if (rule != null)
        {
            Rules.Remove(rule);
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
