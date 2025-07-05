using MediatR;

namespace ChatbotAI.Application.Commands.Chat.RateAiMessage;

public record RateAiMessageCommand(Guid MessageId, int? Rating) : IRequest;