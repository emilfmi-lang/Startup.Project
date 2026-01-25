using FluentValidation;
using Startup_Project.Application.Dtos.GymOpeningHour;

namespace Startup_Project.Application.Validators.GymOpeningHour;

public class OpeningHourDtoValidator:AbstractValidator<OpeningHourDto>
{
    public OpeningHourDtoValidator()
    {
        RuleFor(x => x.OpenTime)
            .NotEmpty().WithMessage("Açılış saatı boş ola bilməz.")
            .Matches(@"^([01]?[0-9]|2[0-3]):[0-5][0-9]$")
            .WithMessage("Açılış saatı düzgün formatda deyil (məs: 09:00).");

        RuleFor(x => x.ClosingTime)
            .NotEmpty().WithMessage("Bağlanış saatı boş ola bilməz.")
            .Matches(@"^([01]?[0-9]|2[0-3]):[0-5][0-9]$")
            .WithMessage("Bağlanış saatı düzgün formatda deyil (məs: 22:00).");

        RuleFor(x => x)
            .Must(x => IsTimeValid(x.OpenTime, x.ClosingTime))
            .When(x => !x.IsClosed)
            .WithMessage("Bağlanış saatı açılış saatından tez ola bilməz.");
    }

    private bool IsTimeValid(string open, string close)
    {
        if (TimeSpan.TryParse(open, out var openTime) && TimeSpan.TryParse(close, out var closeTime))
        {
            return closeTime > openTime;
        }
        return false;
    }
}


