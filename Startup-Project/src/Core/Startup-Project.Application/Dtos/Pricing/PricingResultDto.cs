namespace Startup_Project.Application.Dtos.Pricing;

public class PricingResultDto
{
    public decimal OriginalPrice { get; set; }
    public decimal FinalPrice { get; set; }
    public decimal TotalDiscount { get; set; }
    public List<AppliedDiscountDto> AppliedDiscounts { get; set; } = [];
}
