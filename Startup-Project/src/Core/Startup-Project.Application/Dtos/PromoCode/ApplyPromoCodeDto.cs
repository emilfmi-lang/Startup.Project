namespace Startup_Project.Application.Dtos.PromoCode;

public record ApplyPromoCodeDto(string Code, Guid PackageId);

public record ApplySessionPromoCodeDto(string Code, Guid ScheduledSessionId);
