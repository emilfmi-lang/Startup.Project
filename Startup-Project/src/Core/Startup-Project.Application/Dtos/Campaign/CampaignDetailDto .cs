namespace Startup_Project.Application.Dtos.Campaign;

public class CampaignDetailDto : CampaignListDto
{
    public decimal? MaxDiscountAmount { get; set; }
    public decimal? MinimumOrderAmount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<CampaignRuleDto> Rules { get; set; } = [];
    public List<CampaignUsageDto> RecentUsages { get; set; } = [];
}
