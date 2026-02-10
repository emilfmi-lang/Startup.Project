using FluentValidation;
using Startup_Project.Application.Dtos.Account;

namespace Startup_Project.Application.Validators.Account;

public class ChangePasswordDtoValidator  : AbstractValidator<ChangePasswordDto>
{
    public ChangePasswordDtoValidator()
    {
        RuleFor(x => x.CurrentPassword)
            .NotEmpty().WithMessage("Cari sifre bos ola bilmez.");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("Yeni sifre bos ola bilmez.")
            .MinimumLength(8).WithMessage("Sifre minimum 8 simvol olmalidir.")
            .Matches(@"[A-Z]").WithMessage("Sifrede en az 1 boyuk herf olmalidir.")
            .Matches(@"[a-z]").WithMessage("Sifrede en az 1 kicik herf olmalidir.")
            .Matches(@"\d").WithMessage("Sifrede en az 1 reqem olmalidir.")
            .NotEqual(x => x.CurrentPassword).WithMessage("Yeni sifre kohne sifre ile eyni ola bilmez.");

        RuleFor(x => x.ConfirmNewPassword)
            .NotEmpty().WithMessage("Sifre tesdigi bos ola bilmez.")
            .Equal(x => x.NewPassword).WithMessage("Sifreler uygunlasmalidir.");

    }
}
