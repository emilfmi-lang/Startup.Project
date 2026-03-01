using FluentValidation;
using Startup_Project.Application.Dtos.Admin;

namespace Startup_Project.Application.Validators.Admin;

public class AdminSendMessageDtoValidator : AbstractValidator<AdminSendMessageDto>
{
    public AdminSendMessageDtoValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Ba?l?q bo? ola bilm?z.")
            .MaximumLength(200).WithMessage("Ba?l?q 200 simvoldan çox ola bilm?z.");

        RuleFor(x => x.Body)
            .NotEmpty().WithMessage("Mesaj m?tni bo? ola bilm?z.")
            .MaximumLength(1000).WithMessage("Mesaj m?tni 1000 simvoldan çox ola bilm?z.");
    }
}
