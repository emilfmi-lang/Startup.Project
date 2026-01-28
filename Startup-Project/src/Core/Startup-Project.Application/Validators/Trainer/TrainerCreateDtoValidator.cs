

using AutoMapper.Configuration;
using FluentValidation;
using Startup_Project.Application.Dtos.Trainer;

namespace Startup_Project.Application.Validators.Trainer;

public class TrainerCreateDtoValidator : AbstractValidator<TrainerCreateDto>
{
    public TrainerCreateDtoValidator()
    {
        RuleFor(x => x.UserId)
          .NotEmpty().WithMessage("UserId bos ola bilmez.");

        RuleFor(x => x.Specialization)
            .NotEmpty().WithMessage("Specialization bos ola bilmez.")
            .MaximumLength(200).WithMessage("Specialization 200 simvoldan cox ola bilmez.");

        RuleFor(x => x.Bio)
            .MaximumLength(1000).WithMessage("Bio 1000 simvoldan cox ola bilmez.");

        RuleFor(x => x.HourlyRate)
            .GreaterThanOrEqualTo(0).WithMessage("Saatlig odenis menfiy ola bilmez.");
    }
}
