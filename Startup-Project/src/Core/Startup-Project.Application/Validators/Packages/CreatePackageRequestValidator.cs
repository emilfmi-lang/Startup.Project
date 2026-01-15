

using FluentValidation;
using Startup_Project.Application.Dtos.Packages;

namespace Startup_Project.Application.Validators.Packages;

public class CreatePackageRequestValidator:AbstractValidator<CreatePackageRequestDto>
{
    public CreatePackageRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Price)
            .GreaterThan(0);

        RuleFor(x => x.DurationInDays)
            .GreaterThan(0);

        RuleFor(x => x.GymId)
            .NotEmpty();
    }
}
