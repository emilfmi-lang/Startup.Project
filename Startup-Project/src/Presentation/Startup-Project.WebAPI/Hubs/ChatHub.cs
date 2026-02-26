using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Startup_Project.Application.Abstractions.Chat;
using Startup_Project.Application.Dtos.Chat;
using Startup_Project.Application.Services.Interfaces;
using System.Security.Claims;

namespace Startup_Project.WebAPI.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private readonly IChatService _chatService;
    private readonly IPresenceTracker _presenceTracker;

    public ChatHub(IChatService chatService, IPresenceTracker presenceTracker)
    {
        _chatService = chatService;
        _presenceTracker = presenceTracker;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = GetUserId();
        var isFirstConnection = await _presenceTracker.UserConnected(userId, Context.ConnectionId);

        if (isFirstConnection)
        {
            await Clients.Others.SendAsync("UserIsOnline", userId);
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = GetUserId();
        var isLastConnection = await _presenceTracker.UserDisconnected(userId, Context.ConnectionId);

        if (isLastConnection)
        {
            await Clients.Others.SendAsync("UserIsOffline", userId);
        }

        await base.OnDisconnectedAsync(exception);
    }

    public async Task SendMessage(SendMessageDto dto)
    {
        var messageDto = await _chatService.SendMessageAsync(dto);

        var participantUserIds = await _chatService.GetParticipantUserIdsAsync(dto.ConversationId);
        var currentUserId = GetUserId();

        foreach (var userId in participantUserIds)
        {
            var connections = await _presenceTracker.GetConnectionsForUser(userId);
            foreach (var connId in connections)
            {
                if (userId != currentUserId)
                {
                    var otherDto = new ChatMessageDto
                    {
                        Id = messageDto.Id,
                        SenderId = messageDto.SenderId,
                        SenderFullName = messageDto.SenderFullName,
                        SenderProfileImageUrl = messageDto.SenderProfileImageUrl,
                        Content = messageDto.Content,
                        IsRead = messageDto.IsRead,
                        ReadAt = messageDto.ReadAt,
                        CreatedAt = messageDto.CreatedAt,
                        IsMine = false
                    };
                    await Clients.Client(connId).SendAsync("ReceiveMessage", otherDto);
                }
                else
                {
                    await Clients.Client(connId).SendAsync("ReceiveMessage", messageDto);
                }
            }
        }
    }

    public async Task StartTyping(Guid conversationId)
    {
        var userId = GetUserId();
        var otherConnections = await GetOtherParticipantConnectionsAsync(conversationId, userId);

        foreach (var connId in otherConnections)
        {
            await Clients.Client(connId).SendAsync("UserStartedTyping", new
            {
                ConversationId = conversationId,
                UserId = userId
            });
        }
    }

    public async Task StopTyping(Guid conversationId)
    {
        var userId = GetUserId();
        var otherConnections = await GetOtherParticipantConnectionsAsync(conversationId, userId);

        foreach (var connId in otherConnections)
        {
            await Clients.Client(connId).SendAsync("UserStoppedTyping", new
            {
                ConversationId = conversationId,
                UserId = userId
            });
        }
    }

    public async Task MarkAsRead(Guid conversationId)
    {
        await _chatService.MarkConversationAsReadAsync(conversationId);

        var userId = GetUserId();
        var otherConnections = await GetOtherParticipantConnectionsAsync(conversationId, userId);

        foreach (var connId in otherConnections)
        {
            await Clients.Client(connId).SendAsync("MessagesRead", new
            {
                ConversationId = conversationId,
                ReadByUserId = userId,
                ReadAt = DateTime.UtcNow
            });
        }
    }

    public async Task GetOnlineUsers()
    {
        var onlineUsers = await _presenceTracker.GetOnlineUsers();
        await Clients.Caller.SendAsync("OnlineUsersList", onlineUsers);
    }

    private Guid GetUserId()
    {
        var userIdClaim = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            throw new HubException("?stifad?çi t?yin edil? bilm?di.");
        return userId;
    }

    private async Task<List<string>> GetOtherParticipantConnectionsAsync(Guid conversationId, Guid excludeUserId)
    {
        var participantUserIds = await _chatService.GetParticipantUserIdsAsync(conversationId);
        var connections = new List<string>();

        foreach (var userId in participantUserIds.Where(id => id != excludeUserId))
        {
            var userConnections = await _presenceTracker.GetConnectionsForUser(userId);
            connections.AddRange(userConnections);
        }

        return connections;
    }
}
