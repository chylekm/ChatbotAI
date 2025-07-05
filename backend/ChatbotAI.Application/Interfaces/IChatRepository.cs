using ChatbotAI.Domain.Entities;
using ChatbotAI.Domain.Enums;

namespace ChatbotAI.Application.Interfaces;

public interface IChatRepository
{
    Task<Conversation> CreateConversationAsync(CancellationToken cancellationToken);
    Task<Message> AddMessageAsync(Guid conversationId, MessageRole role, string text, bool isPartial, CancellationToken cancellationToken);
    Task<Message?> GetMessageByIdAsync(Guid messageId, CancellationToken cancellationToken); 
    Task UpdateMessageTextAsync(Guid messageId, string newText, bool isPartial, CancellationToken cancellationToken);
    Task UpdateMessageRatingAsync(Guid messageId, int? rating, CancellationToken cancellationToken);
}