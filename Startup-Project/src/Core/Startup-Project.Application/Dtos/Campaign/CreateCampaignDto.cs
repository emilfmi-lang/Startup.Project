using Startup_Project.Domain.Enums;

namespace Startup_Project.Application.Dtos.Campaign;

public record CreateCampaignDto(
    string Name,
    string Description,
    DiscountType DiscountType,
    decimal DiscountValue,
    decimal? MaxDiscountAmount,
    decimal? MinimumOrderAmount,
    int Priority,
    bool IsStackable,
    TargetType TargetType,
    int? UsageLimit,
    DateTime StartDate,
    DateTime EndDate,
    List<CreateCampaignRuleDto>? Rules
);

public record CreateCampaignRuleDto(
    RuleType RuleType,
    Operator Operator,
    string Value,
    bool IsRequired,
    string? Description
);
