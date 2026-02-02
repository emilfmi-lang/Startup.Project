using FluentValidation;
using Startup_Project.Application.Dtos.PromoCode;

namespace Startup_Project.Application.Validators.Package;

public class ApplyPromoCodeDtoValidator: AbstractValidator<ApplyPromoCodeDto>
{
    public ApplyPromoCodeDtoValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Promokod boş ola bilməz.")
            .MaximumLength(50).WithMessage("Promokod 50 simvoldan çox ola bilməz.")
            .Must(c => !c.Any(char.IsWhiteSpace)).WithMessage("Promokodda boşluq ola bilməz.");

        RuleFor(x => x.PackageId)
            .NotEmpty().WithMessage("Paket seçilməlidir.");

    }
}
