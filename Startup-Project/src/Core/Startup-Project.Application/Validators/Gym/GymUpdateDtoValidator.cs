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
            .MaximumLength(250).WithMessage("Ünvan 250 simvoldan çox olmamal?d?r.")
            .When(x => !string.IsNullOrEmpty(x.Address));

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("T?svir 2000 simvoldan çox olmamal?d?r.")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.Phone)
            .MaximumLength(20).WithMessage("Telefon 20 simvoldan çox olmamal?d?r.")
            .When(x => !string.IsNullOrEmpty(x.Phone));

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("Email format? yanl??d?r.")
            .When(x => !string.IsNullOrEmpty(x.Email));

        RuleFor(x => x.Website)
            .MaximumLength(200).WithMessage("Website 200 simvoldan çox olmamal?d?r.")
            .When(x => !string.IsNullOrEmpty(x.Website));

        RuleFor(x => x.Latitude)
            .InclusiveBetween(-90, 90).WithMessage("Latitude -90 il? 90 aras?nda olmal?d?r.")
            .When(x => x.Latitude.HasValue);

        RuleFor(x => x.Longitude)
            .InclusiveBetween(-180, 180).WithMessage("Longitude -180 il? 180 aras?nda olmal?d?r.")
            .When(x => x.Longitude.HasValue);
    }
}
