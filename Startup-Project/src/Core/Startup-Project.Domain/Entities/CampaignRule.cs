using Startup_Project.Domain.Entities.Common;
using Startup_Project.Domain.Enums;

namespace Startup_Project.Domain.Entities;

public class CampaignRule : BaseEntity
{
    public Guid CampaignId { get; private set; }
    public Campaign Campaign { get; private set; } = null!;
    public RuleType RuleType { get; private set; }
    public Operator Operator { get; private set; }
    public string Value { get; private set; } = null!;
    public bool IsRequired { get; private set; }
    private CampaignRule() { }
    public CampaignRule(Guid campaignId, RuleType ruleType, Operator op, string value, bool isRequired)
    {
        CampaignId = campaignId;
        RuleType = ruleType;
        Operator = op;
        Value = value;
        IsRequired = isRequired;
        CreatedAt = DateTime.UtcNow;
    }
}
