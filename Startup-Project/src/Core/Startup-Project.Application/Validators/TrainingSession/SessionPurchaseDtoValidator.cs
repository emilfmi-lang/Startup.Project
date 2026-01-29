using FluentValidation;
using Startup_Project.Application.Dtos.TrainingSession;

namespace Startup_Project.Application.Validators.TrainingSession;

public class SessionPurchaseDtoValidator : AbstractValidator<SessionPurchaseDto>
{
    public SessionPurchaseDtoValidator()
    {
        RuleFor(x => x.TrainerId)
            .NotEmpty().WithMessage("Trainer ID bos ola bilmez.");
    }
}
