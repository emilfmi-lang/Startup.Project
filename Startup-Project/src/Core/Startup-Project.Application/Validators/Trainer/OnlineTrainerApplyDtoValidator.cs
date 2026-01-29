using FluentValidation;
using Startup_Project.Application.Dtos.Trainer;

namespace Startup_Project.Application.Validators.Trainer;

public class OnlineTrainerApplyDtoValidator : AbstractValidator<OnlineTrainerApplyDto>
{
    public OnlineTrainerApplyDtoValidator()
    {
        RuleFor(x => x.Specialization)
            .NotEmpty().WithMessage("Ixtisas bos ola bilmez.")
            .MaximumLength(200).WithMessage("Ixtisas 200 simvoldan cox ola bilmez.");

        RuleFor(x => x.Bio)
            .MaximumLength(1000).WithMessage("Bio 1000 simvoldan cox ola bilmez.");

        RuleFor(x => x.SessionPrice)
            .GreaterThan(0).WithMessage("Sessiya qiymeti 0-dan boyuk olmalidir.");

        RuleFor(x => x.CertificateUrl)
            .MaximumLength(500).WithMessage("Sertifikat URL 500 simvoldan cox ola bilmez.");

        RuleFor(x => x.VideoIntroUrl)
            .MaximumLength(500).WithMessage("Video URL 500 simvoldan cox ola bilmez.");

        RuleFor(x => x.SocialMediaLinks)
            .MaximumLength(1000).WithMessage("Sosial media 1000 simvoldan cox ola bilmez.");
    }
}
