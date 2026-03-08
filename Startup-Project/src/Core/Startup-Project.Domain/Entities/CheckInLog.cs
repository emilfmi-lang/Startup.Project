using Startup_Project.Domain.Entities.Common;
using Startup_Project.Domain.Entities.Identity;
using Startup_Project.Domain.Enums;

namespace Startup_Project.Domain.Entities;

public class CheckInLog : BaseEntity
{
    public Guid UserId { get; private set; }
    public AppUser User { get; private set; } = null!;

    public Guid GymId { get; private set; }
    public Gym Gym { get; private set; } = null!;

    public Guid? SubscriptionId { get; private set; }
    public Subscription? Subscription { get; private set; }

    public DateTime CheckInTime { get; private set; }
    public DateTime? CheckOutTime { get; private set; }

    public CheckInStatus Status { get; private set; } = CheckInStatus.Success;
    public string? TokenUsed { get; private set; }
    public string? FailureReason { get; private set; }

    private CheckInLog() { }

    public CheckInLog(Guid userId, Guid gymId, Guid? subscriptionId, string? token = null)
    {
        UserId = userId;
        GymId = gymId;
        SubscriptionId = subscriptionId;
        TokenUsed = token;
        CheckInTime = DateTime.UtcNow;
        Status = CheckInStatus.Success;
        CreatedAt = DateTime.UtcNow;
    }

    public void MarkAsSuccess()
    {
        Status = CheckInStatus.Success;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsFailed(string reason)
    {
        Status = CheckInStatus.Failed;
        FailureReason = reason;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsExpired()
    {
        Status = CheckInStatus.Expired;
        FailureReason = "Token vaxtı keçib";
        UpdatedAt = DateTime.UtcNow;
    }

    public void CheckOut()
    {
        CheckOutTime = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}

