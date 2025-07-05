using ChatbotAI.Application.Interfaces;
using ChatbotAI.Domain.Entities;
using ChatbotAI.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace ChatbotAI.Persistence.Repositories;

public class ChatRepository(AppDbContext context) : IChatRepository
{
    private readonly AppDbContext _context = context;

    public async Task<Conversation> CreateConversationAsync(CancellationToken ct)
    {
        var conversation = new Conversation();
        _context.Conversations.Add(conversation);
        await _context.SaveChangesAsync(ct);
        return conversation;
    }

    public async Task<Message> AddMessageAsync(Guid conversationId, MessageRole role, string text, bool isPartial, CancellationToken ct)
    {
        var message = new Message
        {
            ConversationId = conversationId,
            Role = role,
            Text = text,
            Timestamp = DateTime.UtcNow,
            IsPartial = isPartial
        };

        _context.Messages.Add(message);
        await _context.SaveChangesAsync(ct);
        return message;
    }

    public async Task<Message?> GetMessageByIdAsync(Guid messageId, CancellationToken cancellationToken)
    {
        return await _context.Messages.FirstOrDefaultAsync(m => m.Id == messageId, cancellationToken);
    }
    
    public Task UpdateMessageTextAsync(Guid messageId, string newText, bool isPartial, CancellationToken cancellationToken)
    { 
        throw new NotImplementedException();
    }

    public async Task<List<Message>> GetMessagesByConversationIdAsync(Guid conversationId, CancellationToken cancellationToken)
    {
        return await _context.Messages
            .Where(m => m.ConversationId == conversationId)
            .OrderBy(m => m.Timestamp)
            .ToListAsync(cancellationToken);
    }
    
    public async Task UpdateMessageRatingAsync(Guid messageId, int? rating, CancellationToken cancellationToken)
    {
        var message = await _context.Messages.FirstOrDefaultAsync(m => m.Id == messageId, cancellationToken);
        
        if (message is not null)
        {
            message.Rating = rating;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}