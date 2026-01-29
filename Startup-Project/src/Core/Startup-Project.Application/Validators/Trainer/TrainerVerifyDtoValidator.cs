using FluentValidation;
using Startup_Project.Application.Dtos.Trainer;

namespace Startup_Project.Application.Validators.Trainer;

public class TrainerVerifyDtoValidator : AbstractValidator<TrainerVerifyDto>
{
    public TrainerVerifyDtoValidator()
    {
        RuleFor(x => x.RejectionReason)
            .NotEmpty().WithMessage("Redd sebebi bos ola bilmez.")
            .MaximumLength(500).WithMessage("Redd sebebi 500 simvoldan cox ola bilmez.")
            .When(x => !x.Approve);
    }
}
