namespace ChatbotAI.Application.Interfaces;

public interface IAiResponder
{
    IAsyncEnumerable<string> GenerateResponseStreamAsync(string input, CancellationToken cancellationToken = default);
}