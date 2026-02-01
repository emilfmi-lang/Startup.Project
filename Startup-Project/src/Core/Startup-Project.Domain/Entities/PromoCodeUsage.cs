using Startup_Project.Domain.Entities.Common;
using Startup_Project.Domain.Entities.Identity;

namespace Startup_Project.Domain.Entities;

public class PromoCodeUsage : BaseEntity
{
    public Guid PromoCodeId { get; private set; }
    public virtual PromoCode PromoCode { get; private set; }
    public Guid UserId { get; private set; }
    public virtual AppUser User { get; private set; } = null!;
    public Guid PackageId { get; private set; }
    public decimal AppliedDiscount { get; private set; }
    public DateTime UsedAt { get; private set; }
    public PromoCodeUsage() { }
    public PromoCodeUsage(Guid promoCodeId, Guid userId, Guid packageId, decimal appliedDiscount)
    {
        Id = Guid.NewGuid();
        PromoCodeId = promoCodeId;
        UserId = userId;
        PackageId = packageId;
        AppliedDiscount = appliedDiscount;
        UsedAt = DateTime.UtcNow;
        CreatedAt = DateTime.UtcNow;
    }
}
