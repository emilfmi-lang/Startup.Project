using FluentValidation;
using Startup_Project.Application.Dtos.PromoCode;

namespace Startup_Project.Application.Validators.PromoCode;

public class CreatePromoCodeDtoValidator : AbstractValidator<CreatePromoCodeDto>
{
    public CreatePromoCodeDtoValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Promokod bos ola bilmez.")
            .MinimumLength(3).WithMessage("Promokod minimum 3 simvol olmalidir.")
            .MaximumLength(50).WithMessage("Promokod 50 simvoldan cox ola bilmez.")
            .Matches(@"^[A-Za-z0-9_-]+$").WithMessage("Promokod yalniz herf, reqem, tire ve alt xett ola biler.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Tesvir bos ola bilmez.")
            .MaximumLength(500).WithMessage("Tesvir 500 simvoldan cox ola bilmez.");

        RuleFor(x => x.DiscountType)
            .IsInEnum().WithMessage("Endirim tipi duzgun deyil.");

        RuleFor(x => x.TargetType)
            .IsInEnum().WithMessage("Hedef tipi duzgun deyil (Package, Session, All).");

        RuleFor(x => x.DiscountValue)
            .GreaterThan(0).WithMessage("Endirim deyeri 0-dan boyuk olmalidir.")
            .LessThanOrEqualTo(100).When(x => x.DiscountType == Domain.Enums.DiscountType.Percentage)
            .WithMessage("Faiz endirimi 100-den cox ola bilmez.");

        RuleFor(x => x.MinimumOrderAmount)
            .GreaterThanOrEqualTo(0).When(x => x.MinimumOrderAmount.HasValue)
            .WithMessage("Minimum mebleg menfi ola bilmez.");

        RuleFor(x => x.UsageLimit)
            .GreaterThan(0).WithMessage("Istifade limiti 0-dan boyuk olmalidir.");

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Baslama tarixi bos ola bilmez.")
            .GreaterThanOrEqualTo(DateTime.UtcNow.Date).WithMessage("Baslama tarixi kecmis ola bilmez.");

        RuleFor(x => x.EndDate)
            .NotEmpty().WithMessage("Bitme tarixi bos ola bilmez.")
            .GreaterThan(x => x.StartDate).WithMessage("Bitme tarixi baslama tarixinden sonra olmalidir.");
    }
}
