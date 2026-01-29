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
            .NotEmpty().WithMessage("Ixtisas bos ola bilmez.")
            .MaximumLength(200).WithMessage("Ixtisas 200 simvoldan cox ola bilmez.");

        RuleFor(x => x.Bio)
            .MaximumLength(1000).WithMessage("Bio 1000 simvoldan cox ola bilmez.");

        RuleFor(x => x.SessionPrice)
            .GreaterThanOrEqualTo(0).WithMessage("Sessiya qiymeti menfiy ola bilmez.");
    }
}
