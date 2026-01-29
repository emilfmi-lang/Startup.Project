using FluentValidation;
using Startup_Project.Application.Dtos.Account;

namespace Startup_Project.Application.Validators.Account;

public class VerifySmsOtpDtoValidator : AbstractValidator<VerifySmsOtpDto>
{
    public VerifySmsOtpDtoValidator()
    {
        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Telefon nömr?si bo? ola bilm?z.")
            .Matches(@"^\+994[0-9]{9}$").WithMessage("Telefon nömr?si +994XXXXXXXXX format?nda olmal?d?r.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Do?rulama kodu bo? ola bilm?z.")
            .Length(6).WithMessage("Do?rulama kodu 6 r?q?m olmal?d?r.")
            .Matches(@"^[0-9]{6}$").WithMessage("Do?rulama kodu yaln?z r?q?ml?rd?n ibar?t olmal?d?r.");
    }
}
