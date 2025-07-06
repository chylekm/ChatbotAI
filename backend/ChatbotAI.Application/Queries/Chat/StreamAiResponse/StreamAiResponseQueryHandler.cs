using System.Runtime.CompilerServices;
using ChatbotAI.Application.Interfaces;
using ChatbotAI.Domain.Entities;
using ChatbotAI.Domain.Enums;
using FluentValidation;
using MediatR;

namespace ChatbotAI.Application.Queries.Chat.StreamAiResponse;

public class StreamAiResponseQueryHandler : IRequestHandler<StreamAiResponseQuery, (Guid messageId, IAsyncEnumerable<string> stream)>
{
    private readonly IChatRepository _repository;
    private readonly IAiResponder _aiResponder;
    private readonly IValidator<StreamAiResponseQuery> _validator;

    public StreamAiResponseQueryHandler(IChatRepository repository, IAiResponder aiResponder, IValidator<StreamAiResponseQuery> validator)
    {
        _repository = repository;
        _aiResponder = aiResponder;
        _validator = validator;
    }
    
    public async Task<(Guid messageId, IAsyncEnumerable<string> stream)> Handle(StreamAiResponseQuery request, CancellationToken cancellationToken)
    {
        var result = await _validator.ValidateAsync(request, cancellationToken);
        if (!result.IsValid)
            throw new ValidationException(result.Errors);

        return await GenerateStream(request.Message, request.ConversationId, cancellationToken);
    }
    
    private async Task<(Guid messageId, IAsyncEnumerable<string> stream)> GenerateStream(
        string message, Guid? conversationId, CancellationToken cancellationToken)
    {
        var resolvedConversationId = await GetOrCreateConversationIdAsync(conversationId, cancellationToken);

        await AddMessageAsync(
            resolvedConversationId,
            MessageRole.User,
            message,
            isPartial: false,
            cancellationToken);

        var aiMessage = await AddMessageAsync(
            resolvedConversationId,
            MessageRole.AI,
            text: string.Empty,
            isPartial: true,
            cancellationToken);

        var stream = StreamAiResponseAsync(message, aiMessage.Id, cancellationToken);

        return (aiMessage.Id, stream);
    }

    private async Task<Guid> GetOrCreateConversationIdAsync(Guid? conversationId, CancellationToken cancellationToken)
    {
        if (conversationId.HasValue)
            return conversationId.Value;

        var newConversation = await _repository.CreateConversationAsync(cancellationToken);
        return newConversation.Id;
    }

    private Task<Message> AddMessageAsync(Guid conversationId, MessageRole role, string text,
        bool isPartial, CancellationToken cancellationToken)
    {
        return _repository.AddMessageAsync(conversationId, role, text, isPartial, cancellationToken);
    }

    private async IAsyncEnumerable<string> StreamAiResponseAsync(string prompt, Guid aiMessageId,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        string currentText = string.Empty;

        await foreach (var chunk in _aiResponder.GenerateResponseStreamAsync(prompt, cancellationToken))
        {
            if (cancellationToken.IsCancellationRequested)
                yield break;

            currentText += chunk;

            await _repository.UpdateMessageTextAsync(aiMessageId, currentText, isPartial: true, cancellationToken);

            yield return chunk;
        }

        await _repository.UpdateMessageTextAsync(aiMessageId, currentText, isPartial: false, cancellationToken);
    }
}