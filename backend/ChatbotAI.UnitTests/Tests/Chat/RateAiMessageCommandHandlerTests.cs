using ChatbotAI.Application.Commands.Chat.RateAiMessage;
using ChatbotAI.Application.Interfaces;
using ChatbotAI.Domain.Entities;
using ChatbotAI.Domain.Enums;
using FluentValidation;
using FluentValidation.Results;
using Moq;

namespace ChatbotAI.UnitTests.Tests.Chat;

public class RateAiMessageCommandHandlerTests
{
    private readonly Mock<IChatRepository> _repositoryMock = new();
    private readonly Mock<IValidator<RateAiMessageCommand>> _validatorMock = new();

    private RateAiMessageCommandHandler CreateHandler() =>
        new(_repositoryMock.Object, _validatorMock.Object);

    private static RateAiMessageCommand CreateValidCommand() =>
        new(Guid.NewGuid(), 1);
    
    
    [Fact]
    public async Task Handle_ValidCommand_UpdatesRating()
    {
        // Arrange
        var command = CreateValidCommand();
        var handler = CreateHandler();

        _validatorMock.Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _repositoryMock.Setup(r => r.GetMessageByIdAsync(command.MessageId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Message { Id = command.MessageId, Role = MessageRole.AI });

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        _repositoryMock.Verify(r => r.UpdateMessageRatingAsync(command.MessageId, command.Rating, It.IsAny<CancellationToken>()), Times.Once);
    }

    
    [Fact]
    public async Task Handle_InvalidCommand_ThrowsValidationException()
    {
        // Arrange
        var command = CreateValidCommand();
        var handler = CreateHandler();

        _validatorMock.Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(new[] { new ValidationFailure("Rating", "Required") }));

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_MessageNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var command = CreateValidCommand();
        var handler = CreateHandler();

        _validatorMock.Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _repositoryMock.Setup(r => r.GetMessageByIdAsync(command.MessageId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Message?)null);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(command, CancellationToken.None));
        Assert.Contains("not found", ex.Message);
    }

    [Fact]
    public async Task Handle_MessageNotAi_ThrowsInvalidOperationException()
    {
        // Arrange
        var command = CreateValidCommand();
        var handler = CreateHandler();

        _validatorMock.Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _repositoryMock.Setup(r => r.GetMessageByIdAsync(command.MessageId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Message { Id = command.MessageId, Role = MessageRole.User }); // Not AI

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(command, CancellationToken.None));
        Assert.Contains("Only AI responses can be rated", ex.Message);
    }
}