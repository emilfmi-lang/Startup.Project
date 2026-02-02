namespace Startup_Project.Application.Dtos.PromoCode;

public class PromoCodeUsageDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string UserFullName { get; set; } = null!;
    public string UserEmail { get; set; } = null!;
    public Guid PackageId { get; set; }
    public string PackageName { get; set; } = null!;
    public decimal AppliedDiscount { get; set; }
    public DateTime UsedAt { get; set; }
}
