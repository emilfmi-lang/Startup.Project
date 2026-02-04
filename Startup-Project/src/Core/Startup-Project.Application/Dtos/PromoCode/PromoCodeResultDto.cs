namespace Startup_Project.Application.Dtos.PromoCode;

public class PromoCodeResultDto
{
    public bool IsValid { get; set; }
    public decimal OriginalPrice { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal FinalPrice { get; set; }
    public string DiscountType { get; set; } = null!;
    public decimal DiscountValue { get; set; }
    public string Message { get; set; } = null!;
}
