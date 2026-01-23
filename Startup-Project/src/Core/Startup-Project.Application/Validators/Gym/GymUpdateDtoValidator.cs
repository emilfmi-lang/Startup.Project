

using FluentValidation;
using Startup_Project.Application.Dtos.GymDtos;

namespace Startup_Project.Application.Validators.Gym;

public class GymUpdateDtoValidator: AbstractValidator<GymUpdateDto>
{
    public GymUpdateDtoValidator()
    {
        RuleFor(x => x.Address)
            .NotEmpty().WithMessage("Ünvan qeyd edilm?lidir.");
    }
}
