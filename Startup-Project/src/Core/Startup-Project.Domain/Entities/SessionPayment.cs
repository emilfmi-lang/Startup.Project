using Startup_Project.Domain.Entities.Common;
using Startup_Project.Domain.Entities.Identity;
using Startup_Project.Domain.Enums;

namespace Startup_Project.Domain.Entities;

public class SessionPayment : BaseEntity
{
    public Guid StudentId { get; private set; }
    public AppUser Student { get; private set; } = null!;

    public Guid TrainerId { get; private set; }
    public Trainer Trainer { get; private set; } = null!;

    public Guid ScheduledSessionId { get; private set; }
    public ScheduledSession ScheduledSession { get; private set; } = null!;

    public decimal Amount { get; private set; }
    public string OrderId { get; private set; } = null!;
    public string? BankSessionId { get; private set; }
    public PaymentStatus Status { get; private set; } = PaymentStatus.Pending;
    public string? FailureReason { get; private set; }

    private SessionPayment() { }

    public SessionPayment(
        Guid studentId,
        Guid trainerId,
        Guid scheduledSessionId,
        decimal amount)
    {
        StudentId = studentId;
        TrainerId = trainerId;
        ScheduledSessionId = scheduledSessionId;
        Amount = amount;
        OrderId = $"SP-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString()[..8].ToUpperInvariant()}";
        Status = PaymentStatus.Pending;
        CreatedAt = DateTime.UtcNow;
    }

    public void SetBankSessionId(string sessionId)
    {
        BankSessionId = sessionId;
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

    public void MarkAsRefunded()
    {
        Status = PaymentStatus.Refunded;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool IsPending => Status == PaymentStatus.Pending;
    public bool IsSuccessful => Status == PaymentStatus.Successful;
}
