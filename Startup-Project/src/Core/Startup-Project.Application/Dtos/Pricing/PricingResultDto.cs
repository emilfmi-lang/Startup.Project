namespace Startup_Project.Application.Dtos.Pricing;

public class PricingResultDto
{
    public decimal OriginalPrice { get; set; }
    public decimal FinalPrice { get; set; }
    public decimal TotalDiscount { get; set; }
    public decimal CampaignDiscount { get; set; }
    public decimal PromoCodeDiscount { get; set; }
    public List<AppliedDiscountDto> AppliedCampaigns { get; set; } = [];
    public AppliedPromoCodeDto? AppliedPromoCode { get; set; }
    public string Message { get; set; } = null!;
}

public class AppliedPromoCodeDto
{
    public string Code { get; set; } = null!;
    public string DiscountType { get; set; } = null!;
    public decimal DiscountValue { get; set; }
    public decimal DiscountAmount { get; set; }
}
