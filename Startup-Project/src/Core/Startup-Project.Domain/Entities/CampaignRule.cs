using Startup_Project.Domain.Entities.Common;
using Startup_Project.Domain.Enums;
using Startup_Project.Domain.Exceptions;

namespace Startup_Project.Domain.Entities;

public class CampaignRule : BaseEntity
{
    public Guid CampaignId { get; private set; }
    public Campaign Campaign { get; private set; } = null!;
    public RuleType RuleType { get; private set; }
    public Operator Operator { get; private set; }
    public string Value { get; private set; } = null!;
    public bool IsRequired { get; private set; }
    public string? Description { get; private set; }

    private CampaignRule() { }

    public CampaignRule(Guid campaignId, RuleType ruleType, Operator op, string value, bool isRequired, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw BusinessException.ValidationFailed("Qayda dəyəri boş ola bilməz.");

        CampaignId = campaignId;
        RuleType = ruleType;
        Operator = op;
        Value = value;
        IsRequired = isRequired;
        Description = description;
        CreatedAt = DateTime.UtcNow;
    }

    public void Update(string? value, bool? isRequired, string? description)
    {
        if (!string.IsNullOrWhiteSpace(value))
            Value = value;

        if (isRequired.HasValue)
            IsRequired = isRequired.Value;

        if (description != null)
            Description = description;

        UpdatedAt = DateTime.UtcNow;
    }
}
