using Startup_Project.Domain.Entities.Common;
using Startup_Project.Domain.Entities.Identity;
using Startup_Project.Domain.Enums;

namespace Startup_Project.Domain.Entities;

public class TrainingSession : BaseEntity
{
    public Guid TrainerId { get; private set; }
    public Trainer Trainer { get; private set; } = null!;

    public Guid UserId { get; private set; }
    public AppUser User { get; private set; } = null!;

    // GymId nullable - Online trainer ucun NULL olacaq
    public Guid? GymId { get; private set; }
    public Gym? Gym { get; private set; }

    public decimal Price { get; private set; }
    public Guid? PaymentId { get; private set; }
    public Payment? Payment { get; private set; }
    public string? QrToken { get; private set; }
    public DateTime? QrGeneratedAt { get; private set; }
    public SessionStatus Status { get; private set; } = SessionStatus.Pending;
    public DateTime? PaidAt { get; private set; }
    public DateTime? ExpiresAt { get; private set; }
    public DateTime? UsedAt { get; private set; }
    public DateTime? CancelledAt { get; private set; }
    public string? CancellationReason { get; private set; }

    private TrainingSession() { }

    public TrainingSession(Guid trainerId, Guid userId, Guid? gymId, decimal price)
    {
        TrainerId = trainerId;
        UserId = userId;
        GymId = gymId;
        Price = price;
        Status = SessionStatus.Pending;
        CreatedAt = DateTime.UtcNow;
    }

    public void MarkAsPaid(Guid paymentId)
    {
        if (Status != SessionStatus.Pending)
            throw new InvalidOperationException("Yalniz gozleyen sessiya odenile biler.");

        PaymentId = paymentId;
        Status = SessionStatus.Paid;
        PaidAt = DateTime.UtcNow;
        ExpiresAt = DateTime.UtcNow.AddDays(30);
        GenerateQrToken();
        UpdatedAt = DateTime.UtcNow;
    }

    private void GenerateQrToken()
    {
        QrToken = $"TS-{Id}-{Guid.NewGuid().ToString("N")[..8]}-{DateTime.UtcNow.Ticks}";
        QrGeneratedAt = DateTime.UtcNow;
    }

    public void Use()
    {
        if (Status != SessionStatus.Paid)
            throw new InvalidOperationException("Yalniz odenilmis sessiya istifade oluna biler.");

        if (IsExpired())
            throw new InvalidOperationException("Sessiyanin etibarliq muddeti bitib.");

        Status = SessionStatus.Used;
        UsedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool IsExpired()
    {
        return ExpiresAt.HasValue && DateTime.UtcNow > ExpiresAt.Value;
    }

    public void Cancel(string reason)
    {
        if (Status == SessionStatus.Used)
            throw new InvalidOperationException("Istifade olunmus sessiya legv edile bilmez.");

        Status = SessionStatus.Cancelled;
        CancellationReason = reason;
        CancelledAt = DateTime.UtcNow;
        QrToken = null;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsRefunded()
    {
        if (Status != SessionStatus.Cancelled)
            throw new InvalidOperationException("Yalniz legv edilmis sessiyaya refund verilir.");

        Status = SessionStatus.Refunded;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsExpired()
    {
        if (Status == SessionStatus.Paid && IsExpired())
        {
            Status = SessionStatus.Expired;
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public bool ValidateQrToken(string token)
    {
        return Status == SessionStatus.Paid
               && !IsExpired()
               && QrToken == token;
    }
}
