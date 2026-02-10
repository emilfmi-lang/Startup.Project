using FluentValidation;
using Startup_Project.Application.Dtos.Account;

namespace Startup_Project.Application.Validators.Account;

public class AddPaymentMethodDtoValidator : AbstractValidator<AddPaymentMethodDto>
{
    public AddPaymentMethodDtoValidator()
    {
        RuleFor(x => x.CardNumber)
            .NotEmpty().WithMessage("Kart nömr?si bo? ola bilm?z.")
            .CreditCard().WithMessage("Kart nömr?si düzgün deyil.")
            .Must(x => x != null && x.Replace(" ", "").Length is >= 13 and <= 19)
            .WithMessage("Kart nömr?si 13-19 r?q?m aras?nda olmal?d?r.");

        RuleFor(x => x.CardHolderName)
            .NotEmpty().WithMessage("Kart sahibinin ad? bo? ola bilm?z.")
            .MinimumLength(2).WithMessage("Kart sahibinin ad? minimum 2 simvol olmal?d?r.")
            .MaximumLength(100).WithMessage("Kart sahibinin ad? maximum 100 simvol ola bil?r.")
            .Matches(@"^[a-zA-ZÇç????I??iÖö??Üü\s]+$")
            .WithMessage("Kart sahibinin ad? yaln?z h?rfl?rd?n ibar?t olmal?d?r.");

        RuleFor(x => x.ExpiryMonth)
            .InclusiveBetween(1, 12).WithMessage("Ay 1-12 aras?nda olmal?d?r.");

        RuleFor(x => x.ExpiryYear)
            .GreaterThanOrEqualTo(DateTime.UtcNow.Year)
            .WithMessage("Kart?n müdd?ti bitib.");

        RuleFor(x => x)
            .Must(x => !IsExpired(x.ExpiryMonth, x.ExpiryYear))
            .WithMessage("Kart?n müdd?ti bitib.")
            .When(x => x.ExpiryMonth >= 1 && x.ExpiryMonth <= 12 && x.ExpiryYear >= DateTime.UtcNow.Year);
    }

    private static bool IsExpired(int month, int year)
    {
        var now = DateTime.UtcNow;
        return year < now.Year || (year == now.Year && month < now.Month);
    }
}
