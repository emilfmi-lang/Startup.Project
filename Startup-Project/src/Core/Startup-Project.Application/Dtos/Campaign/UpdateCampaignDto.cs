using Startup_Project.Domain.Enums;

namespace Startup_Project.Application.Dtos.Campaign;

public record UpdateCampaignDto(
    string? Name,
    string? Description,
    decimal? DiscountValue,
    decimal? MaxDiscountAmount,
    decimal? MinimumOrderAmount,
    int? Priority,
    bool? IsStackable,
    bool? IsActive,
    int? UsageLimit,
    DateTime? StartDate,
    DateTime? EndDate
);
