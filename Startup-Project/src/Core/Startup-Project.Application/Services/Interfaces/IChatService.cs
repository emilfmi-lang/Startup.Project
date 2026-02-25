using Startup_Project.Application.Dtos.Chat;
using Startup_Project.Application.Models;

namespace Startup_Project.Application.Services.Interfaces;

public interface IChatService
{
    Task<ResponseModel<List<ConversationListDto>>> GetConversationsAsync();
    Task<ResponseModel<ConversationCreatedDto>> GetOrCreateConversationAsync(CreateConversationDto dto);
    Task<ResponseModel<PaginatedMessagesDto>> GetMessagesAsync(Guid conversationId, int page = 1, int pageSize = 30);
    Task<ChatMessageDto> SendMessageAsync(SendMessageDto dto);
    Task<ResponseModel<bool>> MarkConversationAsReadAsync(Guid conversationId);
    Task<ResponseModel<UnreadCountDto>> GetUnreadCountAsync();
    Task<ResponseModel<bool>> ArchiveConversationAsync(Guid conversationId);
        Task<List<Guid>> GetParticipantUserIdsAsync(Guid conversationId);
        Task<ResponseModel<List<ChatUserDto>>> SearchUsersAsync(string? search);
    }
