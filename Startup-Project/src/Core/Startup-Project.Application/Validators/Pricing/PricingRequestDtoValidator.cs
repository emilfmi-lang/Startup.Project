using FluentValidation;
using Startup_Project.Application.Dtos.Pricing;

namespace Startup_Project.Application.Validators.Pricing;

public class PricingRequestDtoValidator : AbstractValidator<PricingRequestDto>
{
    public PricingRequestDtoValidator()
    {
        RuleFor(x => x.PackageId)
            .NotEmpty().WithMessage("Paket seçilm?lidir.");

        RuleFor(x => x.PromoCode)
            .MaximumLength(50)
            .When(x => !string.IsNullOrEmpty(x.PromoCode))
            .WithMessage("Promokod 50 simvoldan çox ola bilm?z.");

        RuleFor(x => x.PromoCode)
            .Must(code => code == null || !code.Any(char.IsWhiteSpace))
            .When(x => !string.IsNullOrEmpty(x.PromoCode))
            .WithMessage("Promokodda bo?luq ola bilm?z.");
    }
}
