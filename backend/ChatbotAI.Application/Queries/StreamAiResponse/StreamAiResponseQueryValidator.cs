using FluentValidation;

namespace ChatbotAI.Application.Queries.StreamAiResponse;

public class StreamAiResponseQueryValidator : AbstractValidator<StreamAiResponseQuery>
{
    public StreamAiResponseQueryValidator()
    {
        RuleFor(x => x.Message)
            .NotEmpty()
            .WithMessage("Message cannot be empty.")
            .MaximumLength(500)
            .WithMessage("Message is too long.");

        RuleFor(x => x.ConversationId)
            .Must(id => id == null || id != Guid.Empty)
            .WithMessage("ConversationId must be null or a valid GUID.");
    }
}