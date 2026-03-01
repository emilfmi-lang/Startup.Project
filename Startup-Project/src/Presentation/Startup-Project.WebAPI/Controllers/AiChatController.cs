using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Startup_Project.Application.Dtos.AI;
using Startup_Project.Application.Services.Interfaces;

namespace Startup_Project.WebAPI.Controllers;

[Route("api/ai")]
[ApiController]
[Authorize]
public class AiChatController(IAiChatService aiChatService) : ControllerBase
{
    [HttpPost("chat")]
    public async Task<IActionResult> SendMessage([FromBody] AiChatRequestDto request)
        => Ok(await aiChatService.SendMessageAsync(request));

    [HttpGet("conversations")]
    public async Task<IActionResult> GetConversations()
        => Ok(await aiChatService.GetConversationsAsync());

    [HttpGet("conversations/{conversationId}/messages")]
    public async Task<IActionResult> GetMessages(
        Guid conversationId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
        => Ok(await aiChatService.GetMessagesAsync(conversationId, page, pageSize));

    [HttpDelete("conversations/{conversationId}")]
    public async Task<IActionResult> DeleteConversation(Guid conversationId)
        => Ok(await aiChatService.DeleteConversationAsync(conversationId));
}
