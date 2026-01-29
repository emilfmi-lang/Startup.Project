using FluentValidation;
using Startup_Project.Application.Dtos.TrainingSession;

namespace Startup_Project.Application.Validators.TrainingSession;

public class SessionVerifyDtoValidator : AbstractValidator<SessionVerifyDto>
{
    public SessionVerifyDtoValidator()
    {
        RuleFor(x => x.QrToken)
            .NotEmpty().WithMessage("QR token bos ola bilmez.")
            .MaximumLength(100).WithMessage("QR token 100 simvoldan cox ola bilmez.");
    }
}
