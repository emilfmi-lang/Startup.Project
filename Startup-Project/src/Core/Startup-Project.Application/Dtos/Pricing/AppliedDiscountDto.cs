namespace Startup_Project.Application.Dtos.Pricing;

public class AppliedDiscountDto
{
    public Guid CampaignId { get; set; }
    public string CampaignName { get; set; } = null!;
    public string DiscountType { get; set; } = null!;
    public decimal DiscountValue { get; set; }
    public decimal DiscountAmount { get; set; }
    public int Priority { get; set; }
}
