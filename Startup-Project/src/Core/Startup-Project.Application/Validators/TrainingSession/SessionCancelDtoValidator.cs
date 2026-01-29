using FluentValidation;
using Startup_Project.Application.Dtos.TrainingSession;

namespace Startup_Project.Application.Validators.TrainingSession;

public class SessionCancelDtoValidator : AbstractValidator<SessionCancelDto>
{
    public SessionCancelDtoValidator()
    {
        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Legv sebebi bos ola bilmez.")
            .MaximumLength(500).WithMessage("Legv sebebi 500 simvoldan cox ola bilmez.");
    }
}
