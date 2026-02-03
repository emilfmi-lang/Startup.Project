namespace Startup_Project.Application.Dtos.Pricing;

public class AppliedDiscountDto
{
    public Guid CampaignId { get; set; }
    public string CampaignName { get; set; } = null!;
    public decimal DiscountAmount { get; set; }
    public decimal OriginalPrice { get; set; }
    public decimal FinalPrice { get; set; }
}
