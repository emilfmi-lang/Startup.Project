using Startup_Project.Domain.Entities.Common;
using Startup_Project.Domain.Entities.Identity;

namespace Startup_Project.Domain.Entities;

public class CheckInToken : BaseEntity
{
    public string Token { get; private set; } = null!;
    public Guid UserId { get; private set; }
    public AppUser User { get; private set; } = null!;

    public Guid GymId { get; private set; }
    public Gym Gym { get; private set; } = null!;

    public Guid SubscriptionId { get; private set; }
    public Subscription Subscription { get; private set; } = null!;

    public DateTime ExpiryDate { get; private set; }
    public bool IsUsed { get; private set; }

    public bool IsExpired => DateTime.UtcNow > ExpiryDate;
    public bool IsValid => !IsUsed && !IsExpired;

    private CheckInToken() { }

    public CheckInToken(Guid userId, Guid gymId, Guid subscriptionId, int expiryMinutes = 5)
    {
        UserId = userId;
        GymId = gymId;
        SubscriptionId = subscriptionId;
        Token = Guid.NewGuid().ToString("N");
        ExpiryDate = DateTime.UtcNow.AddMinutes(expiryMinutes);
        IsUsed = false;
        CreatedAt = DateTime.UtcNow;
    }

    public void MarkAsUsed()
    {
        IsUsed = true;
        UpdatedAt = DateTime.UtcNow;
    }
}
