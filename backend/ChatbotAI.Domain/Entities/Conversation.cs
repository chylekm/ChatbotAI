using ChatbotAI.Domain.Common;

namespace ChatbotAI.Domain.Entities;

public class Conversation : BaseEntity
{
    public ICollection<Message> Messages { get; set; } = new List<Message>();
}