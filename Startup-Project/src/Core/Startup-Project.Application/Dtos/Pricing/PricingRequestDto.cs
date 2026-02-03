namespace Startup_Project.Application.Dtos.Pricing;

public class PricingRequestDto
{
    public Guid UserId { get; set; }
    public Guid PackageId { get; set; }
    public string? PromoCode { get; set; }
}
