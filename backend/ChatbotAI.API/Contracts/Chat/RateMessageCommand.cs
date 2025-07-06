namespace ChatbotAI.API.Contracts.Chat;

public class RateMessageCommand
{
    public int? Rating { get; set; }
    public Guid MessageId { get; set; }
}