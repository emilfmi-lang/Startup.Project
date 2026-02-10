using FluentValidation;
using Startup_Project.Application.Dtos.Account;

namespace Startup_Project.Application.Validators.Account;

public class NotificationSettingsDtoValidator : AbstractValidator<NotificationSettingsDto>  
{
    public NotificationSettingsDtoValidator()
    {
        
    }
}
