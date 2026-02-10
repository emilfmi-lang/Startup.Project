using FluentValidation;
using Startup_Project.Application.Dtos.Account;

namespace Startup_Project.Application.Validators.Account;

public class UserPreferencesDtoValidator  : AbstractValidator<UserPreferencesDto>
{
    private static readonly string[] ValidLanguages = ["az", "en", "ru"];
    private static readonly string[] ValidThemes = ["dark", "light", "system"];
    private static readonly string[] ValidDistanceUnits = ["km", "mi"];
    public UserPreferencesDtoValidator()
    {
        RuleFor(x => x.Language)
            .NotEmpty().WithMessage("Dil secilmelidir.")
            .Must(x => ValidLanguages.Contains(x))
            .WithMessage("Dil az, en ve ya ru olmalidir.");

        RuleFor(x => x.Theme)
            .NotEmpty().WithMessage("Tema secilmelidir.")
            .Must(x => ValidThemes.Contains(x))
            .WithMessage("Tema dark, light ve ya system olmalidir.");

        RuleFor(x => x.DistanceUnit)
            .NotEmpty().WithMessage("Mesafe vahidi secilmelidir.")
            .Must(x => ValidDistanceUnits.Contains(x))
            .WithMessage("Mesafe vahidi km ve ya mi olmalidir.");

        RuleFor(x => x.TimeZone)
            .NotEmpty().WithMessage("Saat qursagi secilmelidir.");
    }
}
