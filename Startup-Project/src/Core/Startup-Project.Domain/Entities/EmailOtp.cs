using Startup_Project.Domain.Entities.Common;

namespace Startup_Project.Domain.Entities;

public class EmailOtp : BaseEntity
{
    public string Email { get; private set; } = null!;
    public string Code { get; private set; } = null!;
    public DateTime ExpiryDate { get; private set; }
    public bool IsUsed { get; private set; }
    public int AttemptCount { get; private set; }

    private EmailOtp() { }

    public EmailOtp(string email, string code, int expiryMinutes = 5)
    {
        Email = email;
        Code = code;
        ExpiryDate = DateTime.UtcNow.AddMinutes(expiryMinutes);
        IsUsed = false;
        AttemptCount = 0;
        CreatedAt = DateTime.UtcNow;
    }

    public void MarkAsUsed()
    {
        IsUsed = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void IncrementAttempt()
    {
        AttemptCount++;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool IsExpired() => DateTime.UtcNow > ExpiryDate;

    public bool IsLocked() => AttemptCount >= 3;

    public bool IsValid() => !IsUsed && !IsExpired() && !IsLocked();
}
