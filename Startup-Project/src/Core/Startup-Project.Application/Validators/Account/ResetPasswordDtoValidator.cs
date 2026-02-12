using FluentValidation;
using Startup_Project.Application.Dtos.Account.Password;

namespace Startup_Project.Application.Validators.Account;

public class ResetPasswordDtoValidator : AbstractValidator<ResetPasswordDto>
{
    public ResetPasswordDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email bo? ola bilm?z.")
            .EmailAddress().WithMessage("Düzgün email format? daxil edin.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Do?rulama kodu bo? ola bilm?z.")
            .Length(6).WithMessage("Do?rulama kodu 6 r?q?m olmal?d?r.")
            .Matches(@"^[0-9]{6}$").WithMessage("Do?rulama kodu yaln?z r?q?ml?rd?n ibar?t olmal?d?r.");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("?ifr? bo? ola bilm?z.")
            .MinimumLength(6).WithMessage("?ifr? ?n az 6 simvol olmal?d?r.");

        RuleFor(x => x.ConfirmPassword)
            .Equal(x => x.NewPassword).WithMessage("?ifr?l?r üst-üst? dü?mür.");
    }
}
