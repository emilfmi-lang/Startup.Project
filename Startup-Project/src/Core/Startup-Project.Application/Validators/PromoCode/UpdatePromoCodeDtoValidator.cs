using FluentValidation;
using Startup_Project.Application.Dtos.PromoCode;

namespace Startup_Project.Application.Validators.PromoCode;

public class UpdatePromoCodeDtoValidator : AbstractValidator<UpdatePromoCodeDto>
{
    public UpdatePromoCodeDtoValidator()
    {
        RuleFor(x => x.Description)
            .MaximumLength(500).When(x => !string.IsNullOrEmpty(x.Description))
            .WithMessage("T?svir 500 simvoldan çox ola bilm?z.");

        RuleFor(x => x.DiscountValue)
            .GreaterThan(0).When(x => x.DiscountValue.HasValue)
            .WithMessage("Endirim d?y?ri 0-dan böyük olmal?d?r.");

        RuleFor(x => x.MinimumOrderAmount)
            .GreaterThanOrEqualTo(0).When(x => x.MinimumOrderAmount.HasValue)
            .WithMessage("Minimum m?bl?? m?nfi ola bilm?z.");

        RuleFor(x => x.UsageLimit)
            .GreaterThan(0).When(x => x.UsageLimit.HasValue)
            .WithMessage("?stifad? limiti 0-dan böyük olmal?d?r.");

        RuleFor(x => x.EndDate)
            .GreaterThan(x => x.StartDate ?? DateTime.UtcNow)
            .When(x => x.EndDate.HasValue)
            .WithMessage("Bitm? tarixi ba?lama tarixind?n sonra olmal?d?r.");
    }
}
