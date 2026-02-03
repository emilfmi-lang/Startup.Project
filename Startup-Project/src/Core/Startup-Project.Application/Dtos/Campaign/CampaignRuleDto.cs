namespace Startup_Project.Application.Dtos.Campaign;

public class CampaignRuleDto
{
    public Guid Id { get; set; }
    public string RuleType { get; set; } = null!;
    public string Operator { get; set; } = null!;
    public string Value { get; set; } = null!;
    public bool IsRequired { get; set; }
    public string? Description { get; set; }
}
