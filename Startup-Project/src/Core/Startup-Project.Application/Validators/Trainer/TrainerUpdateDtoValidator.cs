using FluentValidation;
using Startup_Project.Application.Dtos.Trainer;

namespace Startup_Project.Application.Validators.Trainer;

public class TrainerUpdateDtoValidator : AbstractValidator<TrainerUpdateDto>
{
    public TrainerUpdateDtoValidator()
    {
        RuleFor(x => x.Specialization)
            .MaximumLength(200).WithMessage("Ixtisas 200 simvoldan cox ola bilmez.")
            .When(x => !string.IsNullOrEmpty(x.Specialization));

        RuleFor(x => x.Bio)
            .MaximumLength(1000).WithMessage("Bio 1000 simvoldan cox ola bilmez.");

        RuleFor(x => x.SessionPrice)
            .GreaterThanOrEqualTo(0).WithMessage("Sessiya qiymeti menfiy ola bilmez.")
            .When(x => x.SessionPrice.HasValue);

        RuleFor(x => x.VideoIntroUrl)
            .MaximumLength(500).WithMessage("Video URL 500 simvoldan cox ola bilmez.");

        RuleFor(x => x.SocialMediaLinks)
            .MaximumLength(1000).WithMessage("Sosial media 1000 simvoldan cox ola bilmez.");
    }
}
