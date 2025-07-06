namespace ChatbotAI.API.Contracts.Chat;

public class StreamAiRequest
{
    public string Message { get; set; } = string.Empty;
    public Guid? ConversationId { get; set; }
}