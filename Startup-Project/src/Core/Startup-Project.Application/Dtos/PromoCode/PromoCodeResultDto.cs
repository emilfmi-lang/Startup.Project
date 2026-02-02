namespace Startup_Project.Application.Dtos.PromoCode;

public class PromoCodeResultDto
{
    public bool IsValid { get; set; }
    public decimal OrginalPrice { get; set; }
    public decimal DisCountAmount { get; set; }
    public decimal FinalPrice { get; set; }
    public string DiscountType { get; set; }
    public decimal DiscountValue { get; set; }
    public string Message { get; set; }
}
