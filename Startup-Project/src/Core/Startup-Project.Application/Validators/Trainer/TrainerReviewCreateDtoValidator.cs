using FluentValidation;
using Startup_Project.Application.Dtos.TrainerReview;

namespace Startup_Project.Application.Validators.Trainer;

public class TrainerReviewCreateDtoValidator: AbstractValidator<TrainerReviewCreateDto>
{
    public TrainerReviewCreateDtoValidator()
    {
        RuleFor(x => x.StarCount)
           .InclusiveBetween(1, 5).WithMessage("Ulduz 1-5 arasi olmalidir.");

        RuleFor(x => x.Comment)
            .MaximumLength(500).WithMessage("Comment 500 simvoldan cox ola bilmez.");
    }
}
