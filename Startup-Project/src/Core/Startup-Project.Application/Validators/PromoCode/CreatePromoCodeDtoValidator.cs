using FluentValidation;
using Startup_Project.Application.Dtos.PromoCode;

namespace Startup_Project.Application.Validators.PromoCode;

public class CreatePromoCodeDtoValidator : AbstractValidator<CreatePromoCodeDto>
{
    public CreatePromoCodeDtoValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Promokod bo? ola bilm?z.")
            .MinimumLength(3).WithMessage("Promokod minimum 3 simvol olmal?d?r.")
            .MaximumLength(50).WithMessage("Promokod 50 simvoldan çox ola bilm?z.")
            .Matches(@"^[A-Za-z0-9_-]+$").WithMessage("Promokod yaln?z h?rf, r?q?m, tire v? alt x?tt ola bil?r.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("T?svir bo? ola bilm?z.")
            .MaximumLength(500).WithMessage("T?svir 500 simvoldan çox ola bilm?z.");

        RuleFor(x => x.DiscountType)
            .IsInEnum().WithMessage("Endirim tipi düzgün deyil.");

        RuleFor(x => x.DiscountValue)
            .GreaterThan(0).WithMessage("Endirim d?y?ri 0-dan böyük olmal?d?r.")
            .LessThanOrEqualTo(100).When(x => x.DiscountType == Domain.Enums.DiscountType.Percentage)
            .WithMessage("Faiz endirimi 100-d?n çox ola bilm?z.");

        RuleFor(x => x.MinimumOrderAmount)
            .GreaterThanOrEqualTo(0).When(x => x.MinimumOrderAmount.HasValue)
            .WithMessage("Minimum m?bl?? m?nfi ola bilm?z.");

        RuleFor(x => x.UsageLimit)
            .GreaterThan(0).WithMessage("?stifad? limiti 0-dan böyük olmal?d?r.");

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Ba?lama tarixi bo? ola bilm?z.")
            .GreaterThanOrEqualTo(DateTime.UtcNow.Date).WithMessage("Ba?lama tarixi keçmi? ola bilm?z.");

        RuleFor(x => x.EndDate)
            .NotEmpty().WithMessage("Bitm? tarixi bo? ola bilm?z.")
            .GreaterThan(x => x.StartDate).WithMessage("Bitm? tarixi ba?lama tarixind?n sonra olmal?d?r.");
    }
}
