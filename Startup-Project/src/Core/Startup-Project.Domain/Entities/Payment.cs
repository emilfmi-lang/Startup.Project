using Startup_Project.Domain.Entities.Common;
using Startup_Project.Domain.Entities.Identity;
using Startup_Project.Domain.Enums;

namespace Startup_Project.Domain.Entities;

public class Payment : BaseEntity
{
    public Guid UserId { get;  set; }
    public AppUser User { get; set; } = null!;
    public Guid PackageId { get;  set; }
    public Package Package { get;  set; } = null!;
    public decimal Amount { get;  set; }
    public string? SessionId { get;  set; }
    public PaymentStatus Status { get;  set; } = PaymentStatus.Pending;
    private Payment() { }

    public Payment(Guid userId, Guid packageId, decimal amount, string sessionId)
    {
        UserId = userId;
        PackageId = packageId;
        Amount = amount;
        SessionId = sessionId;
        Status = PaymentStatus.Pending;
    }
    public void SetInitialDetails(Guid userId, decimal amount, string sessionId)
    {
        UserId = userId;
        Amount = amount;
        SessionId = sessionId;
        Status = PaymentStatus.Pending;
    }

    public void MarkAsSuccess() => Status = PaymentStatus.Successful;
    public void MarkAsFailed() => Status = PaymentStatus.Failed;
}
