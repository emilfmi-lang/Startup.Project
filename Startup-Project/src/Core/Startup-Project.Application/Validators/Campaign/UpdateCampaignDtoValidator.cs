using FluentValidation;
using Startup_Project.Application.Dtos.Campaign;

namespace Startup_Project.Application.Validators.Campaign;

public class UpdateCampaignDtoValidator : AbstractValidator<UpdateCampaignDto>
{
    public UpdateCampaignDtoValidator()
    {
        RuleFor(x => x.Name)
            .MaximumLength(200)
            .When(x => !string.IsNullOrEmpty(x.Name))
            .WithMessage("Kampaniya ad? 200 simvoldan çox olmamal?d?r.");

        RuleFor(x => x.Description)
            .MaximumLength(1000)
            .When(x => !string.IsNullOrEmpty(x.Description))
            .WithMessage("T?svir 1000 simvoldan çox olmamal?d?r.");

        RuleFor(x => x.DiscountValue)
            .GreaterThan(0)
            .When(x => x.DiscountValue.HasValue)
            .WithMessage("Endirim d?y?ri 0-dan böyük olmal?d?r.");

        RuleFor(x => x.MaxDiscountAmount)
            .GreaterThan(0)
            .When(x => x.MaxDiscountAmount.HasValue)
            .WithMessage("Maksimum endirim m?bl??i 0-dan böyük olmal?d?r.");

        RuleFor(x => x.MinimumOrderAmount)
            .GreaterThanOrEqualTo(0)
            .When(x => x.MinimumOrderAmount.HasValue)
            .WithMessage("Minimum sifari? m?bl??i m?nfi ola bilm?z.");

        RuleFor(x => x.Priority)
            .GreaterThanOrEqualTo(1)
            .When(x => x.Priority.HasValue)
            .WithMessage("Prioritet ?n az? 1 olmal?d?r.");

        RuleFor(x => x.Priority)
            .LessThanOrEqualTo(100)
            .When(x => x.Priority.HasValue)
            .WithMessage("Prioritet 100-d?n çox ola bilm?z.");

        RuleFor(x => x.UsageLimit)
            .GreaterThan(0)
            .When(x => x.UsageLimit.HasValue)
            .WithMessage("?stifad? limiti 0-dan böyük olmal?d?r.");

        RuleFor(x => x.EndDate)
            .GreaterThan(x => x.StartDate ?? DateTime.UtcNow)
            .When(x => x.EndDate.HasValue)
            .WithMessage("Bitm? tarixi ba?lama tarixind?n sonra olmal?d?r.");
    }
}
