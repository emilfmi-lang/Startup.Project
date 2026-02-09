using FluentValidation;
using Startup_Project.Application.Dtos.History;

namespace Startup_Project.Application.Validators.History;

public class HistoryFilterDtoValidator : AbstractValidator<HistoryFilterDto>
{
    public HistoryFilterDtoValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1)
            .WithMessage("S?hif? nömr?si 1-d?n böyük olmal?d?r.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("S?hif? ölçüsü 1-100 aras?nda olmal?d?r.");

        RuleFor(x => x.EndDate)
            .GreaterThanOrEqualTo(x => x.StartDate)
            .When(x => x.StartDate.HasValue && x.EndDate.HasValue)
            .WithMessage("Biti? tarixi ba?lan??c tarixind?n böyük olmal?d?r.");
    }
}
