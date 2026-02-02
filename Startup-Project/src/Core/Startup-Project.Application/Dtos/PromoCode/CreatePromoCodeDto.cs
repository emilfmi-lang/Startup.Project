using Startup_Project.Domain.Enums;

namespace Startup_Project.Application.Dtos.PromoCode;

public record CreatePromoCodeDto(
    string Code,
    string Description,
    DiscountType DiscountType,
    decimal DiscountValue,
    decimal? MinimumOrderAmount,
    int UsageLimit,
    DateTime StartDate,
    DateTime EndDate,
    Guid? PackageId
);
