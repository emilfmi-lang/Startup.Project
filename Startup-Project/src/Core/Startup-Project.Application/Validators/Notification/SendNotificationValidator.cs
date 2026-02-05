using FluentValidation;
using Startup_Project.Application.Dtos.Notification;

namespace Startup_Project.Application.Validators.Notification;

public class SendNotificationValidator : AbstractValidator<SendNotificationDto>
{
    public SendNotificationValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Başlıq boş ola bilməz.")
            .MaximumLength(200).WithMessage("Başlıq 200 simvoldan çox ola bilməz.");

        RuleFor(x => x.Body)
            .NotEmpty().WithMessage("Mətn boş ola bilməz.")
            .MaximumLength(1000).WithMessage("Mətn 1000 simvoldan çox ola bilməz.");

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Bildiriş tipi düzgün deyil.");

        RuleFor(x => x.Channel)
            .IsInEnum().WithMessage("Bildiriş kanalı düzgün deyil.");

        RuleFor(x => x.ImageUrl)
            .MaximumLength(500).WithMessage("Şəkil URL 500 simvoldan çox ola bilməz.")
            .Must(BeValidUrl).WithMessage("Şəkil URL düzgün format deyil.")
            .When(x => !string.IsNullOrEmpty(x.ImageUrl));

        RuleFor(x => x.ActionUrl)
            .MaximumLength(500).WithMessage("Action URL 500 simvoldan çox ola bilməz.")
            .When(x => !string.IsNullOrEmpty(x.ActionUrl));

        RuleFor(x => x)
            .Must(HaveAtLeastOneTarget).WithMessage("UserId və ya UserIds-dən biri təyin olunmalıdır (broadcast üçün hər ikisi boş ola bilər).");

        RuleFor(x => x.ScheduledFor)
            .GreaterThan(DateTime.UtcNow).WithMessage("Planlaşdırılmış vaxt gələcəkdə olmalıdır.")
            .When(x => x.ScheduledFor.HasValue);
    }

    private static bool BeValidUrl(string? url)
    {
        if (string.IsNullOrEmpty(url)) return true;
        return Uri.TryCreate(url, UriKind.Absolute, out _);
    }

    private static bool HaveAtLeastOneTarget(SendNotificationDto dto)
    {
        // Broadcast üçün hər ikisi null ola bilər
        // Və ya biri təyin olunmalıdır
        return true; // Broadcast da icazəlidir
    }
}
