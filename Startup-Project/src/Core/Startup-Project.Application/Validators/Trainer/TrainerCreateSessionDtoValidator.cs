using FluentValidation;
using Startup_Project.Application.Dtos.Trainer;

namespace Startup_Project.Application.Validators.Trainer;

public class TrainerCreateSessionDtoValidator : AbstractValidator<TrainerCreateSessionDto>
{
    public TrainerCreateSessionDtoValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Seans ad? bo? ola bilm?z.")
            .MinimumLength(3).WithMessage("Seans ad? minimum 3 simvol olmal?d?r.")
            .MaximumLength(200).WithMessage("Seans ad? 200 simvoldan çox ola bilm?z.");

        RuleFor(x => x.Type)
            .NotEmpty().WithMessage("M??q növü bo? ola bilm?z.")
            .MinimumLength(3).WithMessage("M??q növü minimum 3 simvol olmal?d?r.")
            .MaximumLength(200).WithMessage("M??q növü 200 simvoldan çox ola bilm?z.");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Aç?qlama 2000 simvoldan çox ola bilm?z.")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.Date)
            .NotEmpty().WithMessage("Tarix seçilm?lidir.")
            .GreaterThanOrEqualTo(DateTime.UtcNow.Date).WithMessage("Keçmi? tarix? seans t?yin etm?k olmaz.");

        RuleFor(x => x.StartTime)
            .NotEmpty().WithMessage("Ba?lan??c vaxt? bo? ola bilm?z.")
            .Matches(@"^([01]?\d|2[0-3]):[0-5]\d$").WithMessage("Vaxt format? düzgün deyil (HH:mm).");

        RuleFor(x => x.Duration)
            .GreaterThan(0).WithMessage("Müdd?t 0-dan böyük olmal?d?r.")
            .LessThanOrEqualTo(480).WithMessage("Müdd?t 480 d?qiq?d?n çox ola bilm?z.");

        RuleFor(x => x.MaxParticipants)
            .GreaterThan(0).WithMessage("??tirakç? say? 0-dan böyük olmal?d?r.")
            .LessThanOrEqualTo(100).WithMessage("??tirakç? say? 100-d?n çox ola bilm?z.");

        RuleFor(x => x.PricePerPerson)
            .GreaterThanOrEqualTo(0).WithMessage("Qiym?t m?nfi ola bilm?z.");
    }
}
