using Startup_Project.Domain.Entities.Common;

namespace Startup_Project.Domain.Entities;

public class CampaignUsage : BaseEntity
{
    public Guid CampaignId { get; private set; }
    public Campaign Campaign { get; private set; } = null!;
    public Guid UserId { get; private set; }
    public Guid PackageId { get; private set; }
    public decimal OriginalPrice { get; private set; }
    public decimal DiscountAmount { get; private set; }
    public decimal FinalPrice { get; private set; }
    public DateTime AppliedAt { get; private set; } 
    private CampaignUsage() { }
    public CampaignUsage(Guid campaignId, Guid userId, Guid packageId,
                        decimal originalPrice, decimal discountAmount)
    {
        CampaignId = campaignId;
        UserId = userId;
        PackageId = packageId;
        OriginalPrice = originalPrice;
        DiscountAmount = discountAmount;
        FinalPrice = originalPrice - discountAmount;
        AppliedAt = DateTime.UtcNow;
        CreatedAt = DateTime.UtcNow;
    }
}
