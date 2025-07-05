namespace ChatbotAI.Domain.Entities;

public class Conversation
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public ICollection<Message> Messages { get; set; } = new List<Message>();
}