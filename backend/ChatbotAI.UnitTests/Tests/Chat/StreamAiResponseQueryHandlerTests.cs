using System.Runtime.CompilerServices;
using ChatbotAI.Application.Interfaces;
using ChatbotAI.Application.Queries.Chat.StreamAiResponse;
using ChatbotAI.Domain.Entities;
using ChatbotAI.Domain.Enums;
using FluentValidation;
using FluentValidation.Results;
using Moq;

namespace ChatbotAI.UnitTests.Tests.Chat;

public class StreamAiResponseQueryHandlerTests
{
    private readonly Mock<IChatRepository> _repositoryMock = new();
    private readonly Mock<IAiResponder> _aiResponderMock = new();
    private readonly Mock<IValidator<StreamAiResponseQuery>> _validatorMock = new();

    private StreamAiResponseQueryHandler CreateHandler() =>
        new(_repositoryMock.Object, _aiResponderMock.Object, _validatorMock.Object);

    private static StreamAiResponseQuery CreateValidQuery() =>
        new("Hello AI", null);
    
    [Fact]
    public async Task Handle_ValidRequest_ReturnsStreamAndMessageId()
    {
        // Arrange
        var query = CreateValidQuery();
        var handler = CreateHandler();
        var conversationId = Guid.NewGuid();
        var aiMessageId = Guid.NewGuid();

        _validatorMock.Setup(v => v.ValidateAsync(query, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _repositoryMock.Setup(r => r.CreateConversationAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Conversation { Id = conversationId });

        _repositoryMock.Setup(r => r.AddMessageAsync(conversationId, MessageRole.User, query.Message, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Message());

        _repositoryMock.Setup(r => r.AddMessageAsync(conversationId, MessageRole.AI, string.Empty, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Message { Id = aiMessageId });

        _aiResponderMock.Setup(r => r.GenerateResponseStreamAsync(query.Message, It.IsAny<CancellationToken>()))
            .Returns(MockStream(new[] { "Hi", "!" }));

        // Act
        var (messageId, stream) = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(aiMessageId, messageId);

        var result = "";
        await foreach (var chunk in stream)
        {
            result += chunk;
        }

        Assert.Equal("Hi!", result);
    }

    [Fact]
    public async Task Handle_InvalidRequest_ThrowsValidationException()
    {
        // Arrange
        var query = CreateValidQuery();
        var handler = CreateHandler();

        _validatorMock.Setup(v => v.ValidateAsync(query, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(new[] {
                new ValidationFailure(nameof(query.Message), "Message is required")
            }));

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => handler.Handle(query, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_StreamCancelled_StopsEarly()
    {
        // Arrange
        var query = CreateValidQuery();
        var handler = CreateHandler();
        var conversationId = Guid.NewGuid();
        var aiMessageId = Guid.NewGuid();

        _validatorMock.Setup(v => v.ValidateAsync(query, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _repositoryMock.Setup(r => r.CreateConversationAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Conversation { Id = conversationId });

        _repositoryMock.Setup(r => r.AddMessageAsync(It.IsAny<Guid>(), It.IsAny<MessageRole>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Message { Id = aiMessageId });

        _aiResponderMock.Setup(r => r.GenerateResponseStreamAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(MockCancellableStream());

        var cts = new CancellationTokenSource();
        cts.CancelAfter(30); // cancel after 30ms

        // Act
        var (_, stream) = await handler.Handle(query, cts.Token);

        var result = "";
        await foreach (var chunk in stream.WithCancellation(cts.Token))
        {
            result += chunk;
        }

        // Assert – może nie dojść do końca
        Assert.NotEqual("chunk1chunk2chunk3", result);
    }

    private async IAsyncEnumerable<string> MockStream(IEnumerable<string> chunks, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        foreach (var chunk in chunks)
        {
            yield return chunk;
            await Task.Delay(5, cancellationToken);
        }
    }

    private async IAsyncEnumerable<string> MockCancellableStream([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var chunks = new[] { "chunk1", "chunk2", "chunk3" };
        foreach (var chunk in chunks)
        {
            if (cancellationToken.IsCancellationRequested)
                yield break;

            yield return chunk;
            await Task.Delay(20, cancellationToken);
        }
    }

}