using System.Runtime.CompilerServices;
using ChatbotAI.Application.Interfaces;
using ChatbotAI.Domain.Enums;
using MediatR;

namespace ChatbotAI.Application.Queries.StreamAiResponse;

public class StreamAiResponseQueryHandler : IRequestHandler<StreamAiResponseQuery, IAsyncEnumerable<string>>
{
    private readonly IChatRepository _repository;
    private readonly IAiResponder _aiResponder;

    public StreamAiResponseQueryHandler(IChatRepository repository, IAiResponder aiResponder)
    {
        _repository = repository;
        _aiResponder = aiResponder;
    }

    public Task<IAsyncEnumerable<string>> Handle(StreamAiResponseQuery request, CancellationToken cancellationToken)
    {
        return Task.FromResult(GenerateStream(request.Message, request.ConversationId, cancellationToken));
    }

    private async IAsyncEnumerable<string> GenerateStream(string message, Guid? conversationId, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var resolvedConversationId = conversationId ?? (await _repository.CreateConversationAsync(cancellationToken)).Id;

        await _repository.AddMessageAsync(resolvedConversationId, MessageRole.User, message, false, cancellationToken);

        var aiMessage = await _repository.AddMessageAsync(resolvedConversationId, MessageRole.AI, string.Empty, true, cancellationToken);

        string currentText = string.Empty;

        await foreach (var chunk in _aiResponder.GenerateResponseStreamAsync(message, cancellationToken))
        {
            if (cancellationToken.IsCancellationRequested)
                yield break;

            currentText += chunk;

            await _repository.UpdateMessageTextAsync(aiMessage.Id, currentText, true, cancellationToken);

            yield return chunk;
        }

        await _repository.UpdateMessageTextAsync(aiMessage.Id, currentText, false, cancellationToken);
    }
}