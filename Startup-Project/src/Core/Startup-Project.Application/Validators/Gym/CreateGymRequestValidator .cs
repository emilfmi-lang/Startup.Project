using FluentValidation;
using Startup_Project.Application.Dtos.GymDtos;

namespace Startup_Project.Application.Validator.GymValidators;

public class CreateGymRequestValidator: AbstractValidator<CreateGymRequestDto>
{
    public CreateGymRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Address)
            .NotEmpty()
            .MaximumLength(250);

    }
}
