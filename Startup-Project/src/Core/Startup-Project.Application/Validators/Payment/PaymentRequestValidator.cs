using FluentValidation;
using Startup_Project.Application.Dtos.Payment;

namespace Startup_Project.Application.Validators.Payment;

public class PaymentRequestValidator:AbstractValidator<PaymentRequestDto>
{
    public PaymentRequestValidator()
    {
        RuleFor(x => x.PackageId)
            .NotEmpty().WithMessage("Paket seçilməlidir.");
    }
}
