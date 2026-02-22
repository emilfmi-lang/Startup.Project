using Startup_Project.Domain.Entities.Common;
using Startup_Project.Domain.Entities.Identity;
using Startup_Project.Domain.Enums;

namespace Startup_Project.Domain.Entities;

public class Payment : BaseEntity
{
    public Guid UserId { get; private set; }
    public AppUser User { get; private set; } = null!;
    public Guid PackageId { get; private set; }
    public Package Package { get; private set; } = null!;
    public decimal Amount { get; private set; }
    public string? SessionId { get; private set; }
    public PaymentStatus Status { get; private set; } = PaymentStatus.Pending;
    public string? FailureReason { get; private set; }

    private Payment() { }

    public Payment(Guid userId, Guid packageId, decimal amount)
    {
        UserId = userId;
        PackageId = packageId;
        Amount = amount;
        Status = PaymentStatus.Pending;
        CreatedAt = DateTime.UtcNow;
    }

    public void SetBankSessionId(string sessionId)
    {
        SessionId = sessionId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsSuccess()
    {
        Status = PaymentStatus.Successful;
        FailureReason = null;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsFailed(string? reason = null)
    {
        Status = PaymentStatus.Failed;
        FailureReason = reason;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool IsPending => Status == PaymentStatus.Pending;
    public bool IsSuccessful => Status == PaymentStatus.Successful;
}
