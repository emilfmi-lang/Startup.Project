using FluentValidation;
using Startup_Project.Application.Dtos.Chat;

namespace Startup_Project.Application.Validators.Chat;

public class SendMessageDtoValidator : AbstractValidator<SendMessageDto>
{
    public SendMessageDtoValidator()
    {
        RuleFor(x => x.ConversationId)
            .NotEmpty().WithMessage("Söhb?t seçilm?lidir.");

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Mesaj bo? ola bilm?z.")
            .MaximumLength(2000).WithMessage("Mesaj 2000 simvoldan çox ola bilm?z.");
    }
}

public class CreateConversationDtoValidator : AbstractValidator<CreateConversationDto>
{
    public CreateConversationDtoValidator()
    {
        RuleFor(x => x.OtherUserId)
            .NotEmpty().WithMessage("?stifad?çi seçilm?lidir.");
    }
}
