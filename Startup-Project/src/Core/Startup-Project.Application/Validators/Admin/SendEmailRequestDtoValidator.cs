using FluentValidation;
using Startup_Project.Application.Dtos.Admin;

namespace Startup_Project.Application.Validators.Admin;

public class SendEmailRequestDtoValidator : AbstractValidator<SendEmailRequestDto>
{
    public SendEmailRequestDtoValidator()
    {
        RuleFor(x => x.Email)   
            .NotEmpty().WithMessage("Email ünvanı boş ola bilməz.")
            .EmailAddress().WithMessage("Düzgün email formatı daxil edin.")
            .MaximumLength(256).WithMessage("Email ünvanı 256 simvoldan çox ola bilməz.");

        RuleFor(x => x.Subject)
            .NotEmpty().WithMessage("Mövzu boş ola bilməz.")
            .MaximumLength(500).WithMessage("Mövzu 500 simvoldan çox ola bilməz.");

        RuleFor(x => x.Body)
            .NotEmpty().WithMessage("Mətn boş ola bilməz.")
            .MaximumLength(10000).WithMessage("Mətn 10000 simvoldan çox ola bilməz.");
    }
}
