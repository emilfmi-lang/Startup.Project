using FluentValidation;
using Startup_Project.Application.Dtos.Account;

namespace Startup_Project.Application.Validators.Account;

public class SendSmsOtpDtoValidator : AbstractValidator<SendSmsOtpDto>
{
    public SendSmsOtpDtoValidator()
    {
        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Telefon nömr?si bo? ola bilm?z.")
            .Matches(@"^\+994[0-9]{9}$").WithMessage("Telefon nömr?si +994XXXXXXXXX format?nda olmal?d?r.");
    }
}
