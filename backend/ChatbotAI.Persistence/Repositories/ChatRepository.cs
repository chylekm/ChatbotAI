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
    
    public async Task UpdateMessageTextAsync(Guid messageId, string newText, bool isPartial, CancellationToken cancellationToken)
    {
        var message = await _context.Messages.FirstOrDefaultAsync(m => m.Id == messageId, cancellationToken);

        if (message is null)
            throw new InvalidOperationException($"Message with ID {messageId} not found.");

        message.Text = newText;
        message.IsPartial = isPartial;

        await _context.SaveChangesAsync(cancellationToken);
    }
    
    public async Task UpdateMessageRatingAsync(Guid messageId, int? rating, CancellationToken cancellationToken)
    {
        var message = await _context.Messages.FirstOrDefaultAsync(m => m.Id == messageId, cancellationToken);
        if (message is null || message.Role != MessageRole.AI)
            throw new InvalidOperationException("AI message not found.");

        message.Rating = rating;
        await _context.SaveChangesAsync(cancellationToken);
    }
}