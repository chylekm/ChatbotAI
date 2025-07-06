using ChatbotAI.API.Contracts.Chat;
using ChatbotAI.Application.Commands.Chat.RateAiMessage;
using ChatbotAI.Application.Interfaces;
using ChatbotAI.Application.Queries.StreamAiResponse;
using ChatbotAI.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ChatbotAI.API.Controllers;

[ApiController]
[Route("api/chat")]
public class ChatController : ControllerBase
{
    private readonly IMediator _mediator;

    public ChatController(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    [HttpPost("stream")]
    public async Task StreamAiResponse(
        [FromBody] StreamAiRequest request,
        CancellationToken cancellationToken)
    {
        Response.StatusCode = 200;
        Response.ContentType = "text/plain";

        var stream = await _mediator.Send(
            new StreamAiResponseQuery(request.Message, request.ConversationId),
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