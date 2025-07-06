using System.Runtime.CompilerServices;
using ChatbotAI.Application.Interfaces;
using ChatbotAI.Domain.Enums;
using MediatR;

namespace ChatbotAI.Application.Queries.StreamAiResponse;

public class StreamAiResponseQueryHandler : IRequestHandler<StreamAiResponseQuery, (Guid messageId, IAsyncEnumerable<string> stream)>
{
    private readonly IChatRepository _repository;
    private readonly IAiResponder _aiResponder;

    public StreamAiResponseQueryHandler(IChatRepository repository, IAiResponder aiResponder)
    {
        _repository = repository;
        _aiResponder = aiResponder;
    }

    public Task<(Guid messageId, IAsyncEnumerable<string> stream)> Handle(StreamAiResponseQuery request, CancellationToken cancellationToken)
    {
        return GenerateStream(request.Message, request.ConversationId, cancellationToken);
    }

    private async Task<(Guid messageId, IAsyncEnumerable<string> stream)> GenerateStream(string message, Guid? conversationId, CancellationToken cancellationToken)
    {
        var resolvedConversationId = conversationId ?? (await _repository.CreateConversationAsync(cancellationToken)).Id;
        
        await _repository.AddMessageAsync(resolvedConversationId, MessageRole.User, message, false, cancellationToken);
        
        var aiMessage = await _repository.AddMessageAsync(resolvedConversationId, MessageRole.AI, string.Empty, true, cancellationToken);
       
        async IAsyncEnumerable<string> Stream([EnumeratorCancellation] CancellationToken ct)
        {
            string currentText = string.Empty;

            await foreach (var chunk in _aiResponder.GenerateResponseStreamAsync(message, ct))
            {
                if (ct.IsCancellationRequested)
                    yield break;

                currentText += chunk;

                await _repository.UpdateMessageTextAsync(aiMessage.Id, currentText, true, ct);

                yield return chunk;
            }

            await _repository.UpdateMessageTextAsync(aiMessage.Id, currentText, false, ct);
        }

        return (aiMessage.Id, Stream(cancellationToken));
    }
}
