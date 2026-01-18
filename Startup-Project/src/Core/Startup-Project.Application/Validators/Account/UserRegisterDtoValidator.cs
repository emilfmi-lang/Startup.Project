using FluentValidation;
using Startup_Project.Application.Dtos.Account;

namespace Startup_Project.Application.Validators.Account;

public class UserRegisterDtoValidator : AbstractValidator<UserRegisterDto>
{
    public UserRegisterDtoValidator()
    {
        RuleFor(x => x.Email)
            .MinimumLength(5).WithMessage("Email ən az 5 simvol olmalıdır.")
            .NotEmpty().WithMessage("Email boş ola bilməz.")
            .EmailAddress().WithMessage("Düzgün email formatı daxil edin.");

        RuleFor(x => x.UserName)
            .NotEmpty().WithMessage("İstifadəçi adı vacibdir.")
            .MinimumLength(3).WithMessage("İstifadəçi adı ən az 3 simvol olmalıdır.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Şifrə boş ola bilməz.")
            .MinimumLength(6).WithMessage("Şifrə ən az 6 simvol olmalıdır.");

        RuleFor(x => x.ConfirmPassword)
            .Equal(x => x.Password).WithMessage("Şifrələr üst-üstə düşmür.");

        RuleFor(x => x.FullName)
            .MinimumLength(3).WithMessage("Ad və Soyad ən az 3 simvol olmalıdır.")
            .NotEmpty().WithMessage("Ad və Soyad qeyd edilməlidir.");
    }
}
