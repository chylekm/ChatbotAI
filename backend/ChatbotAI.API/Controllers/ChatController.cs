using ChatbotAI.API.Contracts.Chat;
using ChatbotAI.Application.Commands.Chat.RateAiMessage;
using ChatbotAI.Application.Interfaces;
using ChatbotAI.Application.Queries.StreamAiResponse;
using ChatbotAI.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Diagnostics;

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
    public async Task Stream([FromBody] StreamAiResponseQuery query, CancellationToken cancellationToken)
    {
        var (messageId, stream) = await _mediator.Send(query, cancellationToken);

        Response.ContentType = "text/event-stream";

        await Response.WriteAsync($"id:{messageId}\n\n", cancellationToken);
        await Response.Body.FlushAsync(cancellationToken);

        await foreach (var chunk in stream.WithCancellation(cancellationToken))
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            await Response.WriteAsync(chunk, cancellationToken);
            await Response.Body.FlushAsync(cancellationToken);
        }
    }

    [HttpPatch("rate")]
    public async Task<IActionResult> RateAiMessage([FromBody] RateMessageCommand command, CancellationToken cancellationToken)
    {
        await _mediator.Send(new RateAiMessageCommand(command.MessageId, command.Rating), cancellationToken);
        return NoContent();
    }
}