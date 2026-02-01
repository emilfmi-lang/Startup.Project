
using Startup_Project.Domain.Entities.Common;
using Startup_Project.Domain.Enums;

namespace Startup_Project.Domain.Entities;

public class PromoCode : BaseEntity
{
    public string Code { get; private set; }
    public string Description { get; private set; }
    public DiscountType DiscountType { get; private set; }
    public decimal DiscountValue { get; private set; }
    public decimal? MinimumOrderAmount { get; private set; }
    public int UsageLimit { get; private set; }
    public int UsedCount { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public Guid? PackageId { get; private set; }
    public bool IsActive { get; private set; } = true;
    public virtual List<PromoCodeUsage> PromoCodeUsages { get; private set; } = [];
    public PromoCode() { }
    public PromoCode(string code, string description, DiscountType discountType, decimal discountValue,
        DateTime startDate, DateTime endDate, int usageLimit, decimal? minimumOrderAmount = null, Guid? packageId = null)
    {
        Code = code.ToUpperInvariant();
        Description = description;
        DiscountType = discountType;
        DiscountValue = discountValue;
        StartDate = startDate;
        EndDate = endDate;
        UsageLimit = usageLimit;
        MinimumOrderAmount = minimumOrderAmount;
        PackageId = packageId;
        CreatedAt = DateTime.UtcNow;
        if(discountValue <= 0)
            throw new ArgumentException("Discount value must be greater than zero.");
        if(startDate >= endDate)
            throw new ArgumentException("Start date must be earlier than end date.");
    }
    public bool IsValidForDate() => DateTime.UtcNow >= StartDate && DateTime.UtcNow <= EndDate;
    public bool HasAvailableUsage() => UsedCount < UsageLimit;
    public bool IsApplicableToPackage(Guid requestedPackageId) => this.PackageId == null || this.PackageId == requestedPackageId;
    public void IncrementUsedCount()
    {
        UsedCount++;
        UpdatedAt = DateTime.UtcNow;
    }

    public decimal CalculateDiscount(decimal orginalPrice)
    {
        return DiscountType == DiscountType.Percentage
            ? orginalPrice * (DiscountValue / 100)
            : Math.Min(DiscountValue, orginalPrice);
    }
    public bool IsValid() => IsActive&& IsValidForDate() && HasAvailableUsage();
    public bool IsApplicableToOrder(decimal orderAmount)=>
        !MinimumOrderAmount.HasValue|| orderAmount >= MinimumOrderAmount.Value;

    public bool CanBeApplied(Guid packageId, decimal orderAmount)
    {
        return IsActive
        && IsValidForDate()
        && HasAvailableUsage()
        && IsApplicableToPackage(packageId)
        && IsApplicableToOrder(orderAmount);
    }
}
