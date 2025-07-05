using ChatbotAI.Application.Interfaces;
using ChatbotAI.Domain.Enums;
using MediatR;

namespace ChatbotAI.Application.Commands.Chat.RateAiMessage;

public class RateAiMessageCommandHandler : IRequestHandler<RateAiMessageCommand>
{
    private readonly IChatRepository _repository;

    public RateAiMessageCommandHandler(IChatRepository repository)
    {
        _repository = repository;
    }

    public async Task<Unit> Handle(RateAiMessageCommand request, CancellationToken cancellationToken)
    { 
        var message = await _repository.GetMessageByIdAsync(request.MessageId, cancellationToken);

       // if (message == null)
          //  throw new NotFoundException("Message not found");
 
        if (message.Role != MessageRole.AI)
            throw new InvalidOperationException("Only AI responses can be rated");
        
        await _repository.UpdateMessageRatingAsync(request.MessageId, request.Rating, cancellationToken);
        return Unit.Value;
    }
}