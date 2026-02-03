namespace Startup_Project.Application.Dtos.Campaign;

public class CampaignUsageDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string UserFullName { get; set; } = null!;
    public string UserEmail { get; set; } = null!;
    public Guid PackageId { get; set; }
    public string PackageName { get; set; } = null!;
    public decimal OriginalPrice { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal FinalPrice { get; set; }
    public DateTime AppliedAt { get; set; }
}
