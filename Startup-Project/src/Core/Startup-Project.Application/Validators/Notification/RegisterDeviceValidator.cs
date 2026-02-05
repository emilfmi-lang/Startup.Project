using FluentValidation;
using Startup_Project.Application.Dtos.Notification;

namespace Startup_Project.Application.Validators.Notification;

public class RegisterDeviceValidator : AbstractValidator<RegisterDeviceDto>
{
    public RegisterDeviceValidator()
    {
        RuleFor(x => x.DeviceToken)
            .NotEmpty().WithMessage("Device token boş ola bilməz.")
            .MaximumLength(500).WithMessage("Device token 500 simvoldan çox ola bilməz.");

        RuleFor(x => x.Platform)
            .IsInEnum().WithMessage("Platform dəyəri düzgün deyil.");

        RuleFor(x => x.DeviceModel)
            .MaximumLength(100).WithMessage("Device model 100 simvoldan çox ola bilməz.")
            .When(x => !string.IsNullOrEmpty(x.DeviceModel));

        RuleFor(x => x.AppVersion)
            .MaximumLength(50).WithMessage("App version 50 simvoldan çox ola bilməz.")
            .When(x => !string.IsNullOrEmpty(x.AppVersion));
    }
}
