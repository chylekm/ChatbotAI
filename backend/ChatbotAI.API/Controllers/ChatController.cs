using ChatbotAI.Application.Commands.Chat.RateAiMessage;
using ChatbotAI.Application.Interfaces;
using ChatbotAI.Application.Queries.StreamAiResponse;
using ChatbotAI.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ChatbotAI.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IChatRepository _repository;
    private readonly IAiResponder _aiResponder;

    public ChatController(IMediator mediator, IChatRepository repository, IAiResponder aiResponder)
    {
        _mediator = mediator;
        _repository = repository;
        _aiResponder = aiResponder;
    }
    
    [HttpGet("stream")]
    public async Task StreamAiResponse(
        [FromQuery] string userMessage,
        [FromQuery] Guid? conversationId,
        CancellationToken cancellationToken)
    {
        Response.StatusCode = 200;
        Response.ContentType = "text/plain";

        var stream = await _mediator.Send(
            new StreamAiResponseQuery(userMessage, conversationId),
            cancellationToken);

        await foreach (var chunk in stream.WithCancellation(cancellationToken))
        {
            await Response.WriteAsync(chunk, cancellationToken: cancellationToken);
            await Response.Body.FlushAsync(cancellationToken);
        }
    }

    [HttpPut("rate/{messageId:guid}")]
    public async Task<IActionResult> RateAiMessage(Guid messageId, [FromBody] int? rating, CancellationToken cancellationToken)
    {
        await _mediator.Send(new RateAiMessageCommand(messageId, rating), cancellationToken);
        return NoContent();
    }
}