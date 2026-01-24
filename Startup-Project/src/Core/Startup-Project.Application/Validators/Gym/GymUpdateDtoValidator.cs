using FluentValidation;
using Startup_Project.Application.Dtos.GymDtos;

namespace Startup_Project.Application.Validators.Gym;

public class GymUpdateDtoValidator: AbstractValidator<GymUpdateDto>
{
    public GymUpdateDtoValidator()
    {
        RuleFor(x => x.Name)
            .MaximumLength(100).WithMessage("Zal?n ad? 100 simvoldan çox olmamal?d?r.")
            .When(x => !string.IsNullOrEmpty(x.Name));

        RuleFor(x => x.Address)
            .NotEmpty().WithMessage("Ünvan qeyd edilm?lidir.");
    }
}
