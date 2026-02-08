using Startup_Project.Domain.Entities.Common;
using Startup_Project.Domain.Entities.Identity;
using Startup_Project.Domain.Enums;
using Startup_Project.Domain.Exceptions;

namespace Startup_Project.Domain.Entities;

public class Subscription : BaseEntity
{
    public Guid UserId { get; private set; }
    public Guid PackageId { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public SubscriptionStatus Status { get; private set; } = SubscriptionStatus.Active;

    public AppUser User { get; private set; } = null!;
    public Package Package { get; private set; } = null!;

    // Computed property - database-də saxlanmır
    public bool IsActive => Status == SubscriptionStatus.Active && DateTime.UtcNow <= EndDate;
    public bool IsExpired => DateTime.UtcNow > EndDate;

    private Subscription() { }

    public Subscription(Guid userId, Package package, DateTime startDate)
    {
        UserId = userId;
        PackageId = package.Id;
        StartDate = startDate;
        EndDate = startDate.AddDays(package.DurationInDays);
        Status = SubscriptionStatus.Active;
        CreatedAt = DateTime.UtcNow;
    }

    public void Extend(int extraDays)
    {
        if (extraDays <= 0)
            throw BusinessException.ValidationFailed("Əlavə günlər 0-dan böyük olmalıdır.");

        EndDate = EndDate.AddDays(extraDays);
        Status = SubscriptionStatus.Active;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        Status = SubscriptionStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Suspend()
    {
        Status = SubscriptionStatus.Suspended;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        Status = SubscriptionStatus.Active;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsExpired()
    {
        Status = SubscriptionStatus.Expired;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetUser(Guid userId)
    {
        UserId = userId;
    }
}
