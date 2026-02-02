using Startup_Project.Domain.Enums;

namespace Startup_Project.Application.Dtos.PromoCode;

public record UpdatePromoCodeDto(
    string? Description,
    decimal? DiscountValue,
    decimal? MinimumOrderAmount,
    int? UsageLimit,
    DateTime? StartDate,
    DateTime? EndDate,
    bool? IsActive
);
