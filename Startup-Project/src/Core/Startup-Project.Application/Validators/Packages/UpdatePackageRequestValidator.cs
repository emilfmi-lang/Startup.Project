
using FluentValidation;
using Startup_Project.Application.Dtos.Packages;

namespace Startup_Project.Application.Validators.Packages;

public class UpdatePackageRequestValidator : AbstractValidator<UpdatePackageRequestDto>
{
    public UpdatePackageRequestValidator()
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
