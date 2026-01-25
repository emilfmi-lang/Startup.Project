

using FluentValidation;
using Startup_Project.Application.Dtos.GymDtos;

namespace Startup_Project.Application.Validators.Gym;

public class GymCreateDtoValidator: AbstractValidator<GymCreateDto>
{
    public GymCreateDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Zalın adı boş ola bilməz.")
            .MaximumLength(100).WithMessage("Zalın adı 100 simvoldan çox olmamalıdır.");

        RuleFor(x => x.Address)
            .NotEmpty().WithMessage("Ünvan qeyd edilməlidir.");

        RuleFor(x => x.Image)
            .NotNull().WithMessage("Şəkil yüklənməlidir.");
    }
}
