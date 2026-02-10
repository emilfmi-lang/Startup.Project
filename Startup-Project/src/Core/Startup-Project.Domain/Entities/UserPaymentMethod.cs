using Startup_Project.Domain.Entities.Common;
using Startup_Project.Domain.Entities.Identity;
using Startup_Project.Domain.Enums;

namespace Startup_Project.Domain.Entities;

public class UserPaymentMethod : BaseEntity
{
    public Guid UserId { get; private set; }
    public AppUser User { get; private set; } = null!;

    public CardType CardType { get; private set; }
    public string CardHolderName { get; private set; } = null!;
    public string Last4Digits { get; private set; } = null!;
    public int ExpiryMonth { get; private set; }
    public int ExpiryYear { get; private set; }
    public bool IsDefault { get; private set; }

    private UserPaymentMethod() { }

    public UserPaymentMethod(
        Guid userId,
        CardType cardType,
        string cardHolderName,
        string last4Digits,
        int expiryMonth,
        int expiryYear,
        bool isDefault = false)
    {
        UserId = userId;
        CardType = cardType;
        CardHolderName = cardHolderName;
        Last4Digits = last4Digits;
        ExpiryMonth = expiryMonth;
        ExpiryYear = expiryYear;
        IsDefault = isDefault;
    }

    public void SetAsDefault()
    {
        IsDefault = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UnsetDefault()
    {
        IsDefault = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool IsExpired()
    {
        var now = DateTime.UtcNow;
        return ExpiryYear < now.Year || (ExpiryYear == now.Year && ExpiryMonth < now.Month);
    }

    public void Remove()
    {
        IsDeleted = true;
        IsDefault = false;
        UpdatedAt = DateTime.UtcNow;
    }
}
