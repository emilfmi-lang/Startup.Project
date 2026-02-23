
using Startup_Project.Domain.Entities.Common;
using Startup_Project.Domain.Entities.Identity;
using Startup_Project.Domain.Enums;
using Startup_Project.Domain.Exceptions;

namespace Startup_Project.Domain.Entities;

public class PromoCode : BaseEntity
{
    public string Code { get; private set; }
    public string Description { get; private set; }
    public DiscountType DiscountType { get; private set; }
    public decimal DiscountValue { get; private set; }
    public decimal? MinimumOrderAmount { get; private set; }
    public PromoCodeTargetType TargetType { get; private set; } = PromoCodeTargetType.Package;
    public int UsageLimit { get; private set; }
    public int UsedCount { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public Guid? PackageId { get; private set; }
    public virtual Package? Package { get; private set; }
    public Guid CreatedById { get; private set; }
    public virtual AppUser CreatedBy { get; private set; } = null!;
    public Guid? TrainerId { get; private set; }
    public virtual Trainer? Trainer { get; private set; }
    public bool IsActive { get; private set; } = true;
    public virtual List<PromoCodeUsage> PromoCodeUsages { get; private set; } = [];
    public PromoCode() { }

    public PromoCode(string code, string description, DiscountType discountType, decimal discountValue,
        PromoCodeTargetType targetType, DateTime startDate, DateTime endDate, int usageLimit,
        Guid createdById, decimal? minimumOrderAmount = null, Guid? packageId = null, Guid? trainerId = null)
    {
        Code = code.ToUpperInvariant();
        Description = description;
        DiscountType = discountType;
        DiscountValue = discountValue;
        TargetType = targetType;
        StartDate = startDate;
        EndDate = endDate;
        UsageLimit = usageLimit;
        CreatedById = createdById;
        MinimumOrderAmount = minimumOrderAmount;
        PackageId = packageId;
        TrainerId = trainerId;
        CreatedAt = DateTime.UtcNow;

        if (discountValue <= 0)
            throw BusinessException.ValidationFailed("Endirim dəyəri 0-dan böyük olmalıdır.");
        if (startDate >= endDate)
            throw BusinessException.ValidationFailed("Başlama tarixi bitmə tarixindən əvvəl olmalıdır.");
    }

    public bool IsValidForDate() => DateTime.UtcNow >= StartDate && DateTime.UtcNow <= EndDate;
    public bool HasAvailableUsage() => UsedCount < UsageLimit;
    public bool IsApplicableToPackage(Guid requestedPackageId) => PackageId == null || PackageId == requestedPackageId;
    public bool IsForTarget(PromoCodeTargetType requested) => TargetType == PromoCodeTargetType.All || TargetType == requested;
    public bool IsOwnedByTrainer(Guid trainerId) => TrainerId == null || TrainerId == trainerId;

    public void IncrementUsedCount()
    {
        UsedCount++;
        UpdatedAt = DateTime.UtcNow;
    }

    public decimal CalculateDiscount(decimal originalPrice)
    {
        return DiscountType == DiscountType.Percentage
            ? originalPrice * (DiscountValue / 100)
            : Math.Min(DiscountValue, originalPrice);
    }

    public bool IsValid() => IsActive && IsValidForDate() && HasAvailableUsage();

    public bool IsApplicableToOrder(decimal orderAmount) =>
        !MinimumOrderAmount.HasValue || orderAmount >= MinimumOrderAmount.Value;

    public bool CanBeApplied(Guid packageId, decimal orderAmount)
    {
        return IsActive
            && IsValidForDate()
            && HasAvailableUsage()
            && IsApplicableToPackage(packageId)
            && IsApplicableToOrder(orderAmount);
    }

    public bool CanBeAppliedToSession(Guid trainerId, decimal amount)
    {
        return IsActive
            && IsValidForDate()
            && HasAvailableUsage()
            && IsForTarget(PromoCodeTargetType.Session)
            && IsOwnedByTrainer(trainerId)
            && IsApplicableToOrder(amount);
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

    public void Update(string? description, decimal? discountValue, decimal? minimumOrderAmount,
        int? usageLimit, DateTime? startDate, DateTime? endDate)
    {
        if (!string.IsNullOrWhiteSpace(description))
            Description = description;

        if (discountValue.HasValue && discountValue.Value > 0)
            DiscountValue = discountValue.Value;

        if (minimumOrderAmount.HasValue)
            MinimumOrderAmount = minimumOrderAmount.Value;

        if (usageLimit.HasValue && usageLimit.Value > 0)
            UsageLimit = usageLimit.Value;

        if (startDate.HasValue)
            StartDate = startDate.Value;

        if (endDate.HasValue)
            EndDate = endDate.Value;

        UpdatedAt = DateTime.UtcNow;
    }
}
