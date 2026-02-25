using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Startup_Project.Application.Dtos.Chat;
using Startup_Project.Application.Services.Interfaces;

namespace Startup_Project.WebAPI.Controllers;

[Route("api/chat")]
[ApiController]
[Authorize]
public class ChatController(IChatService chatService) : ControllerBase
{
    [HttpGet("conversations")]
    public async Task<IActionResult> GetConversations()
        => Ok(await chatService.GetConversationsAsync());

    [HttpPost("conversations")]
    public async Task<IActionResult> CreateConversation([FromBody] CreateConversationDto dto)
        => Ok(await chatService.GetOrCreateConversationAsync(dto));

    [HttpGet("conversations/{conversationId}/messages")]
    public async Task<IActionResult> GetMessages(
        Guid conversationId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 30)
        => Ok(await chatService.GetMessagesAsync(conversationId, page, pageSize));

    [HttpPost("conversations/{conversationId}/read")]
    public async Task<IActionResult> MarkAsRead(Guid conversationId)
        => Ok(await chatService.MarkConversationAsReadAsync(conversationId));

    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount()
        => Ok(await chatService.GetUnreadCountAsync());

        [HttpDelete("conversations/{conversationId}")]
        public async Task<IActionResult> ArchiveConversation(Guid conversationId)
            => Ok(await chatService.ArchiveConversationAsync(conversationId));

        [HttpGet("users")]
        public async Task<IActionResult> SearchUsers([FromQuery] string? search)
            => Ok(await chatService.SearchUsersAsync(search));
    }
