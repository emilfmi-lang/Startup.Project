using FluentValidation;
using Startup_Project.Application.Dtos.Campaign;
using Startup_Project.Domain.Enums;

namespace Startup_Project.Application.Validators.Campaign;

public class CreateCampaignDtoValidator : AbstractValidator<CreateCampaignDto>
{
    public CreateCampaignDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Kampaniya adı boş ola bilməz.")
            .MaximumLength(200).WithMessage("Kampaniya adı 200 simvoldan çox olmamalıdır.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Təsvir boş ola bilməz.")
            .MaximumLength(1000).WithMessage("Təsvir 1000 simvoldan çox olmamalıdır.");

        RuleFor(x => x.DiscountType)
            .IsInEnum().WithMessage("Endirim tipi düzgün deyil.");

        RuleFor(x => x.DiscountValue)
            .GreaterThan(0).WithMessage("Endirim dəyəri 0-dan böyük olmalıdır.");

        RuleFor(x => x.DiscountValue)
            .LessThanOrEqualTo(100)
            .When(x => x.DiscountType == DiscountType.Percentage)
            .WithMessage("Faiz endirimi 100-dən çox ola bilməz.");

        RuleFor(x => x.MaxDiscountAmount)
            .GreaterThan(0)
            .When(x => x.MaxDiscountAmount.HasValue)
            .WithMessage("Maksimum endirim məbləği 0-dan böyük olmalıdır.");

        RuleFor(x => x.MinimumOrderAmount)
            .GreaterThanOrEqualTo(0)
            .When(x => x.MinimumOrderAmount.HasValue)
            .WithMessage("Minimum sifariş məbləği mənfi ola bilməz.");

        RuleFor(x => x.Priority)
            .GreaterThanOrEqualTo(1).WithMessage("Prioritet ən azı 1 olmalıdır.")
            .LessThanOrEqualTo(100).WithMessage("Prioritet 100-dən çox ola bilməz.");

        RuleFor(x => x.TargetType)
            .IsInEnum().WithMessage("Hədəf tipi düzgün deyil.");

        RuleFor(x => x.UsageLimit)
            .GreaterThan(0)
            .When(x => x.UsageLimit.HasValue)
            .WithMessage("İstifadə limiti 0-dan böyük olmalıdır.");

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Başlama tarixi boş ola bilməz.");

        RuleFor(x => x.EndDate)
            .NotEmpty().WithMessage("Bitmə tarixi boş ola bilməz.")
            .GreaterThan(x => x.StartDate).WithMessage("Bitmə tarixi başlama tarixindən sonra olmalıdır.");

        RuleForEach(x => x.Rules)
            .SetValidator(new CreateCampaignRuleDtoValidator())
            .When(x => x.Rules != null && x.Rules.Count > 0);
    }
}

public class CreateCampaignRuleDtoValidator : AbstractValidator<CreateCampaignRuleDto>
{
    public CreateCampaignRuleDtoValidator()
    {
        RuleFor(x => x.RuleType)
            .IsInEnum().WithMessage("Qayda tipi düzgün deyil.");

        RuleFor(x => x.Operator)
            .IsInEnum().WithMessage("Operator düzgün deyil.");

        RuleFor(x => x.Value)
            .NotEmpty().WithMessage("Qayda dəyəri boş ola bilməz.")
            .MaximumLength(500).WithMessage("Qayda dəyəri 500 simvoldan çox olmamalıdır.");

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .When(x => !string.IsNullOrEmpty(x.Description))
            .WithMessage("Qayda təsviri 500 simvoldan çox olmamalıdır.");
    }
}
