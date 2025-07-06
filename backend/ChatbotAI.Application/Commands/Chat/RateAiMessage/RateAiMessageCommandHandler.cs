using ChatbotAI.Application.Interfaces;
using ChatbotAI.Domain.Enums;
using FluentValidation;
using MediatR;

namespace ChatbotAI.Application.Commands.Chat.RateAiMessage;

public class RateAiMessageCommandHandler : IRequestHandler<RateAiMessageCommand>
{
    private readonly IChatRepository _repository;
    private readonly IValidator<RateAiMessageCommand> _validator;
    
    public RateAiMessageCommandHandler(IChatRepository repository, IValidator<RateAiMessageCommand> validator)
    {
        _repository = repository;
        _validator = validator;
    }

    public async Task<Unit> Handle(RateAiMessageCommand request, CancellationToken cancellationToken)
    { 
        var result = await _validator.ValidateAsync(request, cancellationToken);
        if (!result.IsValid)
            throw new ValidationException(result.Errors);
        
        var message = await _repository.GetMessageByIdAsync(request.MessageId, cancellationToken);
       
        if (message is null)
            throw new InvalidOperationException($"Message with ID {request.MessageId} not found.");

        if (message.Role != MessageRole.AI)
            throw new InvalidOperationException("Only AI responses can be rated");
        
        await _repository.UpdateMessageRatingAsync(request.MessageId, request.Rating, cancellationToken);
        return Unit.Value;
    }
}