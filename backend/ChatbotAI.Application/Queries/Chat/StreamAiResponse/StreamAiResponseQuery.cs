using MediatR;

namespace ChatbotAI.Application.Queries.Chat.StreamAiResponse;

public record StreamAiResponseQuery(string Message, Guid? ConversationId) : IRequest<(Guid messageId, IAsyncEnumerable<string> stream)>;