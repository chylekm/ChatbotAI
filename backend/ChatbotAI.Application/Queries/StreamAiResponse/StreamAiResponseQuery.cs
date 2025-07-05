using MediatR;

namespace ChatbotAI.Application.Queries.StreamAiResponse;

public record StreamAiResponseQuery(string Message, Guid? ConversationId) : IRequest<IAsyncEnumerable<string>>;