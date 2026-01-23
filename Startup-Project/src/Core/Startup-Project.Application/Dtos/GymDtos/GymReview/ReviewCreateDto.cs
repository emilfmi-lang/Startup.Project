using FluentValidation;

namespace Startup_Project.Application.Dtos.GymDtos.GymReview;

public record ReviewCreateDto(Guid GymId, string Comment, int StarCount);

public class ReviewCreateDtoValidator:AbstractValidator<ReviewCreateDto>
{
    public ReviewCreateDtoValidator()
    {
        RuleFor(x => x.StarCount).InclusiveBetween(1, 5).WithMessage("Ulduz 1-5 arası olmalıdır.");
        RuleFor(x => x.Comment).MaximumLength(500).WithMessage("Rəy 500 simvoldan çox ola bilməz.");
        RuleFor(x => x.GymId).NotEmpty().WithMessage("Zal seçilməlidir.");
    }
}
