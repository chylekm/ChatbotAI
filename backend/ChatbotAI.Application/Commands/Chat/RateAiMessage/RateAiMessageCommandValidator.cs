using FluentValidation;

namespace ChatbotAI.Application.Commands.Chat.RateAiMessage;

public class RateAiMessageCommandValidator : AbstractValidator<RateAiMessageCommand>
{
    public RateAiMessageCommandValidator()
    {
        RuleFor(x => x.MessageId)
            .NotEmpty()
            .WithMessage("MessageId is required.");

        RuleFor(x => x.Rating)
            .Must(r => r == null || r == 1 || r == -1)
            .WithMessage("Rating must be 1, -1 or null.");
    }
}