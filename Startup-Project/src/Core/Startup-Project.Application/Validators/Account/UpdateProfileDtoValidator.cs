using FluentValidation;
using Startup_Project.Application.Dtos.Account;

namespace Startup_Project.Application.Validators.Account;

public class UpdateProfileDtoValidator : AbstractValidator<UpdateProfileDto>
{
    public UpdateProfileDtoValidator()
    {
        RuleFor(x => x.FullName)
            .MinimumLength(2).WithMessage("Ad minimum 2 simvol olmalidir.")
            .MaximumLength(100).WithMessage("Ad maximum 100 simvol ola biler.")
            .When(x => !string.IsNullOrEmpty(x.FullName));

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("Email formati duzgun deyil.")
            .When(x => !string.IsNullOrEmpty(x.Email));

        RuleFor(x => x.PhoneNumber)
            .Matches(@"^\+994\d{9}$").WithMessage("Telefon nomresi +994XXXXXXXXX formatinda olmalidir.")
            .When(x => !string.IsNullOrEmpty(x.PhoneNumber));

        RuleFor(x => x.Bio)
            .MaximumLength(250).WithMessage("Bio maximum 250 simvol ola biler.")
            .When(x => !string.IsNullOrEmpty(x.Bio));
    }
}
