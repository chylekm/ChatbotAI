using ChatbotAI.Domain.Enums;

namespace ChatbotAI.Domain.Entities;
public class Message
{
    public Guid Id { get; set; }
    public Guid ConversationId { get; set; }
    public Conversation Conversation { get; set; } = default!;
    public MessageRole Role { get; set; }
    public string Text { get; set; } = "";
    public DateTime Timestamp { get; set; }
    public int? Rating { get; set; } 
    public bool IsPartial { get; set; }
}