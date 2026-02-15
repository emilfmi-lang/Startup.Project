using FluentValidation;
using Startup_Project.Application.Dtos.Admin;

namespace Startup_Project.Application.Validators.Admin;

public class CreateSessionDtoValidator : AbstractValidator<CreateSessionDto>
{
    public CreateSessionDtoValidator()
    {
        RuleFor(x => x.TrainerId)
            .NotEmpty().WithMessage("M??qçi seçilm?lidir.");

        RuleFor(x => x.MemberId)
            .NotEmpty().WithMessage("Üzv seçilm?lidir.");

        RuleFor(x => x.ExerciseType)
            .NotEmpty().WithMessage("M??q növü bo? ola bilm?z.")
            .MinimumLength(3).WithMessage("M??q növü minimum 3 simvol olmal?d?r.")
            .MaximumLength(200).WithMessage("M??q növü 200 simvoldan çox ola bilm?z.");

        RuleFor(x => x.ScheduledTime)
            .NotEmpty().WithMessage("Vaxt seçilm?lidir.")
            .GreaterThan(DateTime.UtcNow).WithMessage("Keçmi? zamana seans t?yin etm?k olmaz.");

        RuleFor(x => x.DurationMinutes)
            .GreaterThan(0).WithMessage("Müdd?t 0-dan böyük olmal?d?r.")
            .LessThanOrEqualTo(480).WithMessage("Müdd?t 480 d?qiq?d?n çox ola bilm?z.");
    }
}
