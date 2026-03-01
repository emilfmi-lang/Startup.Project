using FluentValidation;
using Startup_Project.Application.Dtos.AI;

namespace Startup_Project.Application.Validators.AI;

public class AiChatRequestDtoValidator : AbstractValidator<AiChatRequestDto>
{
    public AiChatRequestDtoValidator()
    {
        RuleFor(x => x.Message)
            .NotEmpty().WithMessage("Mesaj bo? ola bilm?z.")
            .MinimumLength(2).WithMessage("Mesaj ?n az 2 simvol olmal?d?r.")
            .MaximumLength(2000).WithMessage("Mesaj ?n çox 2000 simvol ola bil?r.");
    }
}
